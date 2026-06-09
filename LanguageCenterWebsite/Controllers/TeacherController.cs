using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Controllers
{
    public class TeacherController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        // 1/ Dashboard

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["TeacherID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int teacherId = Convert.ToInt32(Session["TeacherID"]);

            var model = new TeacherDashboardViewModel();
            model.TotalClasses = db.Classes.Count(c => c.teacherID == teacherId);

            model.TotalStudents = (from r in db.Registrations
                                   join c in db.Classes on r.classID equals c.ClassID
                                   where c.teacherID == teacherId
                                   select r.studentID).Distinct().Count();

            model.TeachingSchedule = (from cs in db.ClassSchedules
                                      join c in db.Classes on cs.classID equals c.ClassID
                                      where c.teacherID == teacherId
                                      select new ScheduleViewModel
                                      {
                                          Day = cs.dayOfWeek,
                                          Start = cs.startTime.ToString(),
                                          End = cs.endTime.ToString(),
                                          ClassName = c.className
                                      }).ToList();

            var recentActivities = new List<string>();
            var latestRegistrations = (from r in db.Registrations
                                       join s in db.Students on r.studentID equals s.StudentID
                                       join c in db.Classes on r.classID equals c.ClassID
                                       where c.teacherID == teacherId
                                       orderby r.registrationDate descending
                                       select new { s.fullName, c.className }).Take(5).ToList();

            foreach (var item in latestRegistrations)
            {
                recentActivities.Add($"Học viên {item.fullName} vừa đăng ký vào lớp {item.className} của bạn.");
            }
            if (recentActivities.Count == 0) recentActivities.Add("Hiện tại chưa có hoạt động nào mới.");
            model.RecentActivities = recentActivities;

            return View(model);
        }

        // 2/ My Teaching Classes

        [HttpGet]
        public ActionResult MyClasses()
        {
            int teacherId = 1;

            var rawClasses = (from c in db.Classes
                              where c.teacherID == teacherId
                              select new
                              {
                                  c.ClassID,
                                  c.className,
                                  c.room,
                                  StatusName = c.ClassStatus != null ? c.ClassStatus.statusName : "Không rõ",
                                  Schedules = (from sch in db.ClassSchedules
                                               where sch.classID == c.ClassID
                                               select sch).ToList(),
                                  StudentCount = db.Registrations.Count(r => r.classID == c.ClassID)
                              }).AsEnumerable();

            List<TeacherClassViewModel> classList = (from c in rawClasses
                                                     select new TeacherClassViewModel
                                                     {
                                                         ClassID = c.ClassID,
                                                         ClassName = c.className,
                                                         Room = c.room,
                                                         Status = c.StatusName,
                                                         StudentCount = c.StudentCount,
                                                         Schedule = c.Schedules.Select(sch => sch.dayOfWeek + " (" +
                                                           (sch.startTime.HasValue ? sch.startTime.Value.ToString(@"hh\:mm") : "00:00") + " - " +
                                                           (sch.endTime.HasValue ? sch.endTime.Value.ToString(@"hh\:mm") : "00:00") + ")")
                                                       .FirstOrDefault() ?? "Chưa xếp lịch"
                                                     }).ToList();

            return View(classList);

        }

        // 3/ CLASS STUDENTS

        [HttpGet]
        public ActionResult ClassStudents(int? classId)
        {
            if (classId == null) return RedirectToAction("MyClasses");
            int id = classId.Value;

            // 1. KHAI BÁO BIẾN TRƯỚC (để nó tồn tại trong toàn bộ hàm)
            List<ClassStudentViewModel> studentList = new List<ClassStudentViewModel>();

            // 2. Lấy thông tin lớp học (đã xử lý logic ngày tạo)
            var firstRegistrationDate = db.Registrations
                                           .Where(r => r.classID == id)
                                           .OrderBy(r => r.registrationDate)
                                           .Select(r => r.registrationDate)
                                           .FirstOrDefault();

            var currentClass = db.Classes.FirstOrDefault(c => c.ClassID == id);
            if (currentClass != null)
            {
                ViewBag.ClassName = currentClass.className;
                ViewBag.Room = currentClass.room;
                ViewBag.TeacherName = currentClass.Teacher?.fullName ?? "Chưa có";
                ViewBag.Status = currentClass.ClassStatus?.statusName ?? "Chưa rõ";
                ViewBag.ProgramName = currentClass.Program?.programName ?? "Chưa rõ";
                ViewBag.CreatedDate = firstRegistrationDate.HasValue
                                      ? firstRegistrationDate.Value.ToString("dd/MM/yyyy")
                                      : "Chưa có học viên";
            }

            // 3. Thực hiện truy vấn dữ liệu thô
            var data = (from r in db.Registrations
                        where r.classID == id
                        join s in db.Students on r.studentID equals s.StudentID
                        select new
                        {
                            s.StudentID,
                            s.fullName,
                            r.registrationDate,
                            r.status
                        }).ToList();

            // 4. GÁN DỮ LIỆU VÀO BIẾN ĐÃ KHAI BÁO
            studentList = data.Select(x => new ClassStudentViewModel
            {
                StudentID = x.StudentID,
                FullName = x.fullName,
                RegDate = x.registrationDate.HasValue ? x.registrationDate.Value.ToString("dd/MM/yyyy") : "",
                PaymentStatus = x.status ?? "Chưa rõ"
            }).ToList();

            // Bây giờ studentList đã tồn tại ở đây!
            return View(studentList);
        }
        // 2.1/ ADD CLASS (Thêm lớp học mới - 1.0 Điểm)
        [HttpGet]
        public ActionResult CreateClass()
        {
            // 1. Lấy danh sách trạng thái lớp
            ViewBag.statusID = new SelectList(db.ClassStatus, "statusID", "statusName");

            // 2. BỔ SUNG: Lấy danh sách Chương trình học từ bảng Program đổ vào Dropdown
            // (Bạn kiểm tra xem trong file .dbml viết là ProgramID hay programID nhé)
            ViewBag.programID = new SelectList(db.Programs, "ProgramID", "programName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClass(Class newClass)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Mặc định gán lớp này cho Giáo viên hiện tại (ID = 1)
                    newClass.teacherID = Convert.ToInt32(Session["TeacherID"]);

                    db.Classes.InsertOnSubmit(newClass);
                    db.SubmitChanges(); // Lưu xuống SQL Server

                    TempData["Message"] = "Thêm mới lớp học thành công!";
                    TempData["Status"] = "success";
                    return RedirectToAction("MyClasses");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống khi thêm lớp: " + ex.Message);
                }
            }

            // Nếu lỗi, nạp lại dữ liệu cho các Dropdown công thức cũ
            ViewBag.statusID = new SelectList(db.ClassStatus, "statusID", "statusName", newClass.statusID);
            ViewBag.programID = new SelectList(db.Programs, "ProgramID", "programName", newClass.programID);
            return View(newClass);
        }

        // ==========================================
        // 2.2/ EDIT CLASS (Sửa thông tin lớp - 1.0 Điểm)
        // ==========================================
        [HttpGet]
        public ActionResult EditClass(int id)
        {
            var editClass = db.Classes
                              .FirstOrDefault(c => c.ClassID == id);

            if (editClass == null)
            {
                return HttpNotFound();
            }

            ViewBag.StatusID = new SelectList(
                db.ClassStatus,
                "StatusID",
                "statusName",
                editClass.statusID);

            return View(editClass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClass(Class updatedClass)
        {
            var existingClass = db.Classes
                                  .FirstOrDefault(c => c.ClassID == updatedClass.ClassID);

            if (existingClass == null)
            {
                return HttpNotFound();
            }

            try
            {
                existingClass.className = updatedClass.className;
                existingClass.room = updatedClass.room;
                existingClass.statusID = updatedClass.statusID;

                db.SubmitChanges();

                TempData["Message"] = "Update class successfully!";
                return RedirectToAction("MyClasses");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.StatusID = new SelectList(
                db.ClassStatus,
                "StatusID",
                "statusName",
                updatedClass.statusID);

            return View(updatedClass);
        }

        // 2.3/ UPDATE STATUS (Đổi nhanh trạng thái lớp - 1.0 Điểm)
        public ActionResult ToggleStatus(int id)
        {
            var targetClass = db.Classes.FirstOrDefault(c => c.ClassID == id);
            if (targetClass != null)
            {
                // Logic đổi qua đổi lại trạng thái lớp (Ví dụ: ID 1 là Active, ID 2 là Inactive)
                // Bạn hãy check lại bảng ClassStatus của bạn xem ID của các trạng thái là số mấy nha
                if (targetClass.statusID == 1)
                {
                    targetClass.statusID = 2; // Đổi sang Tạm dừng/Kết thúc
                }
                else
                {
                    targetClass.statusID = 1; // Đổi về Đang mở
                }

                db.SubmitChanges();
                TempData["Message"] = "Đã thay đổi trạng thái hoạt động của lớp " + targetClass.className + "!";
                TempData["Status"] = "success";
            }

            return RedirectToAction("MyClasses");
        }


        // 2.4/ DELETE CLASS (Xóa lớp học - 1.0 Điểm)

        public ActionResult DeleteClass(int id)
        {
            try
            {
                var targetClass =
                    db.Classes.FirstOrDefault(c => c.ClassID == id);

                if (targetClass == null)
                {
                    return HttpNotFound();
                }

                targetClass.statusID = 4; // Deleted

                db.SubmitChanges();

                TempData["Message"] = "Class deleted successfully!";
                TempData["Status"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Status"] = "danger";
            }

            return RedirectToAction("MyClasses");
        }


        // 4/ MANAGE CLASS MATERIALS (Lấy dữ liệu bảng Material chuẩn database)

        [HttpGet]
        public ActionResult ManageClassMaterials(int? classId)
        {
            if (classId == null)
            {
                return RedirectToAction("MyClasses");
            }

            var materials = db.Materials
                              .Where(m => m.classID == classId.Value)
                              .ToList()
                              .Select(m => new ClassMaterial
                              {
                                  MaterialID = m.MaterialID.ToString(),
                                  ClassID = m.classID,
                                  FileName = m.materialName,
                                  FilePath = m.documentPath,
                                  UploadDate = m.uploadDate.HasValue
                                                ? m.uploadDate.Value.ToString("dd/MM/yyyy")
                                                : "",
                                  FileSize = "N/A"
                              })
                              .ToList();

            var currentClass = db.Classes
                                 .SingleOrDefault(c => c.ClassID == classId.Value);

            ViewBag.ClassID = classId.Value;
            ViewBag.ClassName = currentClass != null
                                ? currentClass.className
                                : "Unknown Class";

            return View(materials);
        }

        // 5/ PLACEMENT TEST RESULTS

        [HttpGet]
        public ActionResult PlacementTestResults()
        {
            var testResults =
                (from pt in db.PlacementTests
                 join s in db.Students
                 on pt.studentID equals s.StudentID

                 select new PlacementTestViewModel
                 {
                     TestID = pt.TestID,
                     StudentName = s.fullName,

                     TestDate =
                         pt.testDate.HasValue
                         ? pt.testDate.Value.ToString("dd/MM/yyyy")
                         : "Chưa thi",

                     SuggestedLevel =
                         string.IsNullOrEmpty(pt.suggestedLevel)
                         ? "Chưa xếp"
                         : pt.suggestedLevel,ResultScore = pt.resultScore.HasValue
                                                         ? Convert.ToDouble(pt.resultScore.Value)
                                                         : 0,Status = string.IsNullOrEmpty(pt.status) ? "Pending"
                                                         : pt.status
                 })
                 .ToList();

            return View(testResults);
        }
        // 6/ STUDENT FEEDBACK
        public ActionResult StudentFeedback(string statusFilter)
        {
            var consultations = db.Consultations.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                consultations = consultations
                                .Where(c => c.requestStatus == statusFilter);
            }

            var model = consultations
                        .OrderByDescending(c => c.ConsultationID)
                        .ToList()
                        .Select(c => new StudentFeedbackViewModel
                        {
                            FeedbackID = c.ConsultationID,
                            StudentName = c.fullName,
                            FeedbackContent = c.question,
                            FeedbackDate = new DateTime(),
                            Status = c.requestStatus,
                            Rating = 0
                        })
                        .ToList();

            ViewBag.TotalFeedback = db.Consultations.Count();
            ViewBag.PendingFeedback = db.Consultations.Count( c => c.requestStatus == "Chưa xử lý");
            ViewBag.RepliedFeedback = db.Consultations.Count( c => c.requestStatus == "Đã liên hệ");
            ViewBag.CurrentFilter = statusFilter;
            return View(model);
        }

        [HttpGet]
        public ActionResult UpdateTeacherImage()
        {
            int teacherId = 1;

            var teacher =
                db.Teachers.FirstOrDefault(
                    t => t.TeacherID == teacherId);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            ViewBag.TeacherName = teacher.fullName;
            ViewBag.CurrentImg = teacher.avatar;

            return View();
        }

        // POST: Xử lý nhận file ảnh chân dung, đổi tên tránh trùng và lưu vào Content/Img
        [HttpPost]
        public ActionResult UpdateTeacherImage(HttpPostedFileBase imageFile)
        {
            int teacherId = 1;

            var teacher =
                db.Teachers.FirstOrDefault(
                    t => t.TeacherID == teacherId);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            if (imageFile != null &&
                imageFile.ContentLength > 0)
            {
                string extension =
                    Path.GetExtension(imageFile.FileName)
                        .ToLower();

                if (extension == ".jpg" ||
                    extension == ".jpeg" ||
                    extension == ".png")
                {
                    string newFileName =
                        "teacher_" +
                        teacherId + "_" +
                        Guid.NewGuid()
                            .ToString()
                            .Substring(0, 8)
                        + extension;

                    string folderPath =
                        Server.MapPath("~/Content/Img/");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fullPath =
                        Path.Combine(
                            folderPath,
                            newFileName);

                    imageFile.SaveAs(fullPath);

                    string imgPath =
                        "/Content/Img/" + newFileName;

                    teacher.avatar = imgPath;

                    db.SubmitChanges();

                    ViewBag.Message =
                        "Update image successfully";

                    ViewBag.Status = "success";
                }
                else
                {
                    ViewBag.Message =
                        "Only JPG, JPEG, PNG";

                    ViewBag.Status = "warning";
                }
            }

            ViewBag.TeacherName = teacher.fullName;
            ViewBag.CurrentImg = teacher.avatar;

            return View();
        }
    }
}