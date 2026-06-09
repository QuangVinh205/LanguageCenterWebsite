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
            int teacherId = 1;

            var model = new TeacherDashboardViewModel();
            model.TotalClasses = db.Classes.Count(c => c.teacherID == teacherId);

            model.TotalStudents = (from r in db.Registrations
                                   join c in db.Classes on r.classID equals c.ClassID
                                   where c.teacherID == teacherId
                                   select r.studentID).Distinct().Count();

            model.TeachingSchedule = (from cs in db.ClassSchedules
                                      join c in db.Classes on cs.classID equals c.ClassID
                                      where c.teacherID == teacherId
                                      select new ScheduleViewModel // Ép vào Model cụ thể
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

            // Truyền trực tiếp đối tượng model qua View độc lập
            return View(model);
        }

        // 2/ My Teaching Classes
        [HttpGet]
        public ActionResult MyClasses()
        {
            int teacherId = 1;

            // BƯỚC 1: Lấy dữ liệu từ SQL Server lên bộ nhớ RAM bằng .AsEnumerable()
            var rawClasses = (from c in db.Classes
                              where c.teacherID == teacherId
                              select new
                              {
                                  c.ClassID,
                                  c.className,
                                  c.room,
                                  // Kiểm tra xem Property liên kết của bạn là ClassStatus hay ClassStatus1 nhé
                                  StatusName = c.ClassStatus != null ? c.ClassStatus.statusName : "Không rõ",

                                  // Lấy danh sách lịch học thô của lớp này dưới dạng danh sách
                                  Schedules = (from sch in db.ClassSchedules
                                               where sch.classID == c.ClassID
                                               select sch).ToList(),

                                  StudentCount = db.Registrations.Count(r => r.classID == c.ClassID)
                              }).AsEnumerable(); // Ép xuống bộ nhớ C# từ đây

            // BƯỚC 2: Định dạng chuỗi thời gian bằng C# thuần để dọn sạch đống chữ "Jan 1 1900"
            List<TeacherClassViewModel> classList = (from c in rawClasses
                                                     select new TeacherClassViewModel
                                                     {
                                                         ClassID = c.ClassID,
                                                         ClassName = c.className,
                                                         Room = c.room,
                                                         Status = c.StatusName,
                                                         StudentCount = c.StudentCount,

                                                         // Xử lý chuỗi thời gian hh:mm cực đẹp
                                                         Schedule = c.Schedules.Select(sch => sch.dayOfWeek + " (" +
                                                             (sch.startTime.HasValue ? sch.startTime.Value.ToString(@"hh\:mm") : "00:00") + " - " +
                                                             (sch.endTime.HasValue ? sch.endTime.Value.ToString(@"hh\:mm") : "00:00") + ")")
                                                         .FirstOrDefault() ?? "Chưa xếp lịch"
                                                     }).ToList();

            return View(classList);
        }
        [HttpGet]
        public ActionResult ClassStudents(int? classId)
        {
            if (classId == null)
            {
                return RedirectToAction("MyClasses");
            }

            int id = classId.Value;

            var currentClass = db.Classes.FirstOrDefault(c => c.ClassID == id);
            ViewBag.ClassID = id;
            ViewBag.ClassName = currentClass != null ? currentClass.className : "Không rõ lớp";

            // BƯỚC 1: Lấy dữ liệu thô từ SQL Server lên bộ nhớ RAM bằng AsEnumerable()
            var rawStudents = (from r in db.Registrations
                               where r.classID == id
                               join s in db.Students on r.studentID equals s.StudentID
                               select new
                               {
                                   s.StudentID,
                                   s.fullName,
                                   r.registrationDate,
                                   r.status // Lấy trạng thái thanh toán từ bảng Registration để gán vào PaymentStatus
                               }).AsEnumerable();

            // BƯỚC 2: Đổ dữ liệu vào ViewModel khớp chính xác 100% tên thuộc tính của bạn
            List<ClassStudentViewModel> studentList = (from s in rawStudents
                                                       select new ClassStudentViewModel
                                                       {
                                                           StudentID = s.StudentID,
                                                           FullName = s.fullName,

                                                           // Đổi tên thành RegDate cho đúng chuẩn Model của bạn này:
                                                           RegDate = s.registrationDate != null ? s.registrationDate.Value.ToString("dd/MM/yyyy") : "",

                                                           // Gán trạng thái thanh toán (ví dụ: "Paid")
                                                           PaymentStatus = s.status ?? "Chưa rõ",

                                                           // Thuộc tính này chưa dùng đến ở trang này thì tạm để trống hoặc mặc định
                                                           AttendanceStatus = "Có mặt"
                                                       }).ToList();

            return View(studentList);
        }
        [HttpGet]
        public ActionResult ManageMaterials(int? classId)
        {
            var currentClass = db.Classes.FirstOrDefault(c => c.ClassID == classId);
            ViewBag.ClassID = classId;
            ViewBag.ClassName = currentClass != null ? currentClass.className : "Không rõ lớp";

            // BƯỚC 1: Lấy dữ liệu thô từ SQL Server lên bộ nhớ RAM bằng .AsEnumerable()
            var rawMaterials = (from m in db.Materials
                                where m.classID == classId
                                select m).AsEnumerable();

            // BƯỚC 2: Định dạng ngày tháng an toàn tuyệt đối trên bộ nhớ C#
            List<ClassMaterialViewModel> materials = (from m in rawMaterials
                                                      select new ClassMaterialViewModel
                                                      {
                                                          MaterialID = m.MaterialID,
                                                          ClassID = m.classID,
                                                          MaterialName = m.materialName,
                                                          Description = m.description,
                                                          DocumentPath = m.documentPath,
                                                          // Chạy mượt mà trên RAM, không bao giờ lo lỗi dịch SQL nữa:
                                                          UploadDate = m.uploadDate != null ? m.uploadDate.Value.ToString("dd/MM/yyyy") : ""
                                                      }).ToList();

            return View(materials);
        }

        // 4.2. Thêm mới tài liệu & Upload File Documents
        [HttpPost]
        public ActionResult AddMaterial(ClassMaterialViewModel model, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    // Tạo thư mục lưu trữ trên server máy chủ nếu chưa có sẵn
                    string folderPath = Server.MapPath("~/Uploads/Materials/");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // Gắn nhãn mốc Ticks thời gian để bảo đảm file tải lên không bị trùng tên đè dữ liệu
                    fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(uploadFile.FileName);
                    string physicalPath = Path.Combine(folderPath, fileName);
                    uploadFile.SaveAs(physicalPath);
                    fileName = "/Uploads/Materials/" + fileName;
                }

                var newMaterial = new Material
                {
                    classID = model.ClassID,
                    materialName = model.MaterialName,
                    description = model.Description,
                    documentPath = fileName,
                    uploadDate = DateTime.Now
                };

                db.Materials.InsertOnSubmit(newMaterial);
                db.SubmitChanges();
            }
            return RedirectToAction("ManageMaterials", new { classId = model.ClassID });
        }

        // 4.3. Xóa tài liệu khỏi danh sách và máy chủ
        [HttpPost]
        public ActionResult DeleteMaterial(int materialId, int classId)
        {
            var material = db.Materials.FirstOrDefault(m => m.MaterialID == materialId);
            if (material != null)
            {
                // Thực hiện dọn dẹp file vật lý trong thư mục hệ thống để tránh đầy ổ cứng
                if (!string.IsNullOrEmpty(material.documentPath))
                {
                    string physicalPath = Server.MapPath("~" + material.documentPath);
                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                }

                db.Materials.DeleteOnSubmit(material);
                db.SubmitChanges();
            }
            return RedirectToAction("ManageMaterials", new { classId = classId });
        }
        [HttpGet]
        public ActionResult PlacementTestResults()
        {
            // BƯỚC 1: Lấy dữ liệu thô từ bảng PlacementTest và kết nối qua bảng Student để lấy tên
            var rawTests = (from pt in db.PlacementTests
                            join s in db.Students on pt.studentID equals s.StudentID
                            select new
                            {
                                pt.TestID,
                                s.fullName,
                                pt.testDate,
                                pt.suggestedLevel,
                                pt.resultScore,
                                pt.status
                            }).AsEnumerable(); // Đưa dữ liệu lên RAM để định dạng chuỗi an toàn

            // BƯỚC 2: Đổ dữ liệu vào đúng cấu trúc Model đã tạo ở Bước 1
            List<PlacementTestViewModel> testResults = (from pt in rawTests
                                                        select new PlacementTestViewModel
                                                        {
                                                            TestID = pt.TestID,
                                                            StudentName = pt.fullName,
                                                            // Ép kiểu ngày tháng sang chữ dd/MM/yyyy mượt mà trên RAM
                                                            TestDate = pt.testDate != null ? pt.testDate.Value.ToString("dd/MM/yyyy") : "Chưa thi",
                                                            SuggestedLevel = pt.suggestedLevel ?? "Chưa xếp",
                                                            // Ép kiểu số decimal từ SQL sang double của C#
                                                            ResultScore = pt.resultScore != null ? (double)pt.resultScore.Value : 0.0,
                                                            Status = pt.status ?? "Chưa rõ"
                                                        }).ToList();

            return View(testResults);
        }
        [HttpGet]
        public ActionResult StudentFeedback()
        {
            // BƯỚC 1: Lấy dữ liệu thô từ bảng Consultation dưới SQL Server lên RAM
            var rawFeedback = (from c in db.Consultations
                               select c).AsEnumerable();

            // BƯỚC 2: Đổ dữ liệu vào ViewModel và xử lý định dạng hiển thị bằng C#
            int fakeDay = 1; // Tạo ngày mẫu tăng dần để hiển thị cho đẹp mắt
            List<StudentFeedbackViewModel> feedbackList = (from f in rawFeedback
                                                           select new StudentFeedbackViewModel
                                                           {
                                                               FeedbackID = f.ConsultationID,
                                                               StudentName = f.fullName,
                                                               FeedbackContent = f.question ?? "Không có nội dung câu hỏi.",
                                                               // Mồi số sao ngẫu nhiên dựa trên ID để giao diện sinh động
                                                               Rating = (f.ConsultationID % 2 == 0) ? 5 : 4,
                                                               // Tạo chuỗi ngày tháng mẫu dd/MM/yyyy mượt mà trên RAM
                                                               FeedbackDate = (fakeDay++).ToString("00") + "/06/2026",
                                                               Status = f.requestStatus ?? "Pending"
                                                           }).ToList();

            return View(feedbackList);
        }

    }
}