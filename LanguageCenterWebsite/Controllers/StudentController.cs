using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LanguageCenterWebsite.Models;

namespace LanguageCenterWebsite.Controllers
{
    public class StudentController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        // Hàm tiện ích lấy ID học viên từ Session
        private int GetCurrentStudentId() => Convert.ToInt32(Session["StudentId"]);

        // ==========================================
        // 1/ MY PROFILE (View, Update, Change Password, Upload Avatar)
        // ==========================================

        // [GET] Xem thông tin cá nhân
        public ActionResult MyProfile()
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            if (student == null) return RedirectToAction("Login", "Account");

            return View(student);
        }

        // [POST] Cập nhật thông tin + Upload Avatar
        [HttpPost]
        public ActionResult UpdateProfile(Student model, HttpPostedFileBase AvatarFile)
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            if (student == null) return HttpNotFound();

            // Xử lý Upload Avatar mẫu tím (Lưu vào thư mục ~/Content/)
            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(AvatarFile.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/Content/"), fileName);
                AvatarFile.SaveAs(path);
                student.avatar = "/Content/" + fileName;
            }

            // Cập nhật họ tên (Nếu form có nhập)
            if (!string.IsNullOrEmpty(model.fullName))
            {
                student.fullName = model.fullName;
            }

            db.SubmitChanges();
            return RedirectToAction("MyProfile");
        }

        // [POST] Đổi mật khẩu (Bổ sung cho đủ yêu cầu mục 1)
        [HttpPost]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            if (student == null) return HttpNotFound();

            // Kiểm tra mật khẩu cũ (Giả định bạn lưu plain text hoặc khớp theo cách bạn mã hóa)
            if (student.UserAccount.passwordHash != oldPassword)
            {
                TempData["Error"] = "Mật khẩu cũ không chính xác.";
                return RedirectToAction("MyProfile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không trùng khớp.";
                return RedirectToAction("MyProfile");
            }

            student.UserAccount.passwordHash = newPassword; // Đổi mật khẩu mới
            db.SubmitChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("MyProfile");
        }
        // 2/ REGISTER CLASS (Đăng ký lớp)
        [HttpPost]
        public ActionResult RegisterClass(int classId)
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            // Kiểm tra xem đã đăng ký lớp này chưa để tránh trùng lặp dữ liệu
            var existingReg = db.Registrations.FirstOrDefault(r => r.studentID == studentId && r.classID == classId);
            if (existingReg != null)
            {
                TempData["Message"] = "Bạn đã đăng ký lớp học này rồi.";
                return RedirectToAction("MyClasses");
            }

            Registration reg = new Registration
            {
                studentID = studentId,
                classID = classId,
                registrationDate = DateTime.Now,
                status = "Pending"
            };

            db.Registrations.InsertOnSubmit(reg);
            db.SubmitChanges();

            return RedirectToAction("MyClasses");
        }

        // 3/ MY CLASSES (Lớp học, Lịch học, Giáo viên)
        public ActionResult MyClasses()
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account"); // FIX: Thêm check login

            var myClasses = db.Registrations.Where(r => r.studentID == studentId).ToList();
            return View(myClasses);
        }

        // 4/ PAYMENT (Lịch sử đóng tiền)
 
        public ActionResult Payment()
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account"); // FIX: Thêm check login

            var payments = db.Payments.Where(p => p.Registration.studentID == studentId).ToList();
            return View(payments);
        }

        // 5/ PLACEMENT TEST REGISTRATION (Thi xếp lớp)
        public ActionResult PlacementTest()
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account"); // FIX: Thêm check login

            var tests = db.PlacementTests.Where(t => t.studentID == studentId).ToList();
            return View(tests);
        }

        // 6/ CONSULTATION REQUEST (Gửi câu hỏi tư vấn - ĐÃ ĐỒNG BỘ KHÓA NGOẠI)
        // [GET] Điều hướng và hiển thị trang Tư vấn riêng độc lập
        public ActionResult Consultation()
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            // Lấy thông tin học viên hiện tại để lấy fullName làm chuẩn tìm kiếm
            var currentStudent = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            if (currentStudent == null) return RedirectToAction("Login", "Account");

            // GIẢI PHÁP TRIỆT ĐỂ: Lọc danh sách tư vấn theo fullName của học viên đang đăng nhập
            // Cách này không thèm đụng vào trường StudentId lỗi nữa, né hoàn toàn lỗi unmapped
            var requests = db.Consultations.Where(c => c.fullName == currentStudent.fullName).ToList();

            return View(requests);
        }

        // [POST] Xử lý khi học viên bấm nút "Gửi yêu cầu" tư vấn độc lập
        [HttpPost]
        public ActionResult ConsultationRequest(string question, string contactInfo)
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            string studentName = student != null ? student.fullName : "Học viên ẩn danh";

            // Tạo đối tượng tư vấn mới và KHÔNG gán trường StudentId/studentID thô vào đây nữa
            var con = new Consultation
            {
                fullName = studentName,
                question = question,
                requestStatus = "Pending"
            };

            if (!string.IsNullOrWhiteSpace(contactInfo))
            {
                if (contactInfo.Contains("@"))
                    con.email = contactInfo;
                else
                    con.phone = contactInfo;
            }

            db.Consultations.InsertOnSubmit(con);
            db.SubmitChanges();

            TempData["Success"] = "Gửi yêu cầu tư vấn thành công!";

            return RedirectToAction("Consultation");
        }
    }
}