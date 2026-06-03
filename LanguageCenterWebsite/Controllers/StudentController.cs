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

        private int GetCurrentStudentId() => Convert.ToInt32(Session["StudentId"]);

        // 1/ View Profile
        public ActionResult MyProfile()
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            return View(student);
        }

        // 1/ Update Profile + Upload Avatar
        [HttpPost]
        public ActionResult UpdateProfile(Student model, HttpPostedFileBase AvatarFile)
        {
            int studentId = GetCurrentStudentId();
            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);

            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(AvatarFile.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/Content/"), fileName);
                AvatarFile.SaveAs(path);
                student.avatar = "/Content/" + fileName;
            }

            student.fullName = model.fullName;
            db.SubmitChanges();
            return RedirectToAction("MyProfile");
        }

        // 2/ Register Class (Đăng ký lớp)
        [HttpPost]
        public ActionResult RegisterClass(int classId)
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            Registration reg = new Registration { studentID = studentId, classID = classId, registrationDate = DateTime.Now, status = "Pending" };
            db.Registrations.InsertOnSubmit(reg);
            db.SubmitChanges();

            return RedirectToAction("MyClasses");
        }

        // 3/ My Classes (Lớp học, Lịch học, Giáo viên)
        public ActionResult MyClasses()
        {
            int studentId = GetCurrentStudentId();
            var myClasses = db.Registrations.Where(r => r.studentID == studentId).ToList();
            return View(myClasses);
        }

        // 4/ Payment (Lịch sử đóng tiền)
        public ActionResult Payment()
        {
            int studentId = GetCurrentStudentId();
            var payments = db.Payments.Where(p => p.Registration.studentID == studentId).ToList();
            return View(payments);
        }

        // 5/ Placement Test Registration (Thi xếp lớp)
        public ActionResult PlacementTest()
        {
            int studentId = GetCurrentStudentId();
            var tests = db.PlacementTests.Where(t => t.studentID == studentId).ToList();
            return View(tests);
        }

        // 6/ Consultation Request (Gửi câu hỏi tư vấn)
        [HttpPost]
        public ActionResult ConsultationRequest(string question, string contactInfo)
        {
            int studentId = GetCurrentStudentId();
            if (studentId == 0) return RedirectToAction("Login", "Account");

            // 1. Lấy thông tin học viên để lấy ra fullName (bắt buộc trong DB của bạn)
            var student = db.Students.FirstOrDefault(s => s.StudentID == studentId);
            string studentName = student != null ? student.fullName : "Học viên ẩn danh";

            // 2. Khởi tạo đối tượng Consultation với các trường dữ liệu của bạn
            var con = new Consultation
            {
                StudentId = studentId,       // Hãy check lại nếu chữ s viết thường (studentID) theo chuẩn của bạn
                fullName = studentName,      // THÊM DÒNG NÀY: Sửa lỗi chặn NULL từ SQL Server
                question = question,
                requestStatus = "Pending"    // Trạng thái theo đúng thuộc tính bạn đặt
            };

            // 3. Logic phân tách contactInfo thành email hoặc phone của bạn
            if (!string.IsNullOrWhiteSpace(contactInfo))
            {
                if (contactInfo.Contains("@"))
                    con.email = contactInfo;
                else
                    con.phone = contactInfo;
            }

            db.Consultations.InsertOnSubmit(con);
            db.SubmitChanges();
            return RedirectToAction("MyProfile");
        }
    }
}