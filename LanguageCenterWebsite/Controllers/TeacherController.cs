using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Sử dụng Navigation Property (Thuộc tính liên kết tự động của LINQ to SQL)
            // Thay vì join thủ công, ta gọi thẳng c.ClassStatus.statusName hoặc c.ClassStatus1.statusName
            List<TeacherClassViewModel> classList = (from c in db.Classes
                                                     where c.teacherID == teacherId
                                                     select new TeacherClassViewModel
                                                     {
                                                         ClassID = c.ClassID,
                                                         ClassName = c.className,
                                                         Room = c.room,

                                                         // Lấy trạng thái thông qua bảng liên kết tự động cs
                                                         // Lưu ý: Nhấn dấu chấm (.) sau c xem nó gợi ý là 'ClassStatus' hay 'ClassStatus1' nhé
                                                         Status = c.ClassStatus != null ? c.ClassStatus.statusName : "Không rõ",

                                                         StudentCount = db.Registrations.Count(r => r.classID == c.ClassID),

                                                         Schedule = (from sch in db.ClassSchedules
                                                                     where sch.classID == c.ClassID
                                                                     select sch.dayOfWeek + " (" + sch.startTime + " - " + sch.endTime + ")")
                                                                    .FirstOrDefault() ?? "Chưa xếp lịch"
                                                     }).ToList();

            return View(classList);
        }
        // 3/ View Class Students
        [HttpGet]
        public ActionResult ClassStudents(int? classId) // Thêm dấu hỏi (?) để cho phép null
        {
            // 1. Kiểm tra nếu classId bị null (do gõ sai URL hoặc không truyền từ View)
            if (classId == null)
            {
                // Chủ động chuyển hướng an toàn về trang danh sách lớp để không bị crash
                return RedirectToAction("MyClasses");
            }

            // 2. Nếu có classId, ép về kiểu int bình thường bằng thuộc tính .Value để chạy LINQ
            int id = classId.Value;

            var currentClass = db.Classes.FirstOrDefault(c => c.ClassID == id);
            ViewBag.ClassName = currentClass != null ? currentClass.className : "Không rõ lớp";

            List<ClassStudentViewModel> studentList = (from r in db.Registrations
                                                       where r.classID == id
                                                       join s in db.Students on r.studentID equals s.StudentID
                                                       select new ClassStudentViewModel
                                                       {
                                                           StudentID = s.StudentID,
                                                           FullName = s.fullName,
                                                           RegDate = r.registrationDate != null ? r.registrationDate.Value.ToString("dd/MM/yyyy") : "",
                                                           PaymentStatus = r.status,
                                                           AttendanceStatus = (r.status == "Confirmed" || r.status == "Paid") ? "Đang học (Active)" : "Chờ xếp lớp"
                                                       }).ToList();

            return View(studentList);
        }
    }
}