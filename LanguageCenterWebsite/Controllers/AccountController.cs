using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Controllers
{
    public class AccountController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        // ==========================================
        // HÀM TIỆN ÍCH: GỬI MÃ OTP QUA GMAIL
        // ==========================================
        private void SendVerificationEmail(string toEmail, string otpCode)
        {
            // Thiết lập email gửi đi (Thay bằng email của bạn)
            var fromAddress = new MailAddress("ouyu96502@gmail.com", "Language Center");
            var toAddress = new MailAddress(toEmail);

            // Mật khẩu ứng dụng Gmail (App Password) gồm 16 ký tự tạo từ bảo mật tài khoản Google
            string fromPassword = "wgwd cppm qkxp ttzg";

            string subject = "Account Verification Code";
            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; max-width: 500px; margin: 0 auto; border-radius: 8px;'>
                    <h2 style='color: #6f42c1; text-align: center;'>Welcome to Language Center!</h2>
                    <p>Thank you for registering. Here is your email verification code:</p>
                    <div style='background: #f4f4f4; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #333; border-radius: 4px; margin: 20px 0;'>
                        {otpCode}
                    </div>
                    <p style='color: #777; font-size: 12px; text-align: center;'>This code will expire shortly. Please do not share this code with anyone.</p>
                </div>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body, IsBodyHtml = true })
            {
                smtp.Send(message);
            }
        }

        // ==========================================
        // CHỨC NĂNG ĐĂNG KÝ (REGISTER)
        // ==========================================

        // GET: Register
        public ActionResult Register() => View();

        // POST: Register (BƯỚC 1: Sinh OTP, gửi Mail và lưu tạm vào Session)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng lặp trước khi gửi mã
                if (db.UserAccounts.Any(u => u.email == model.Email))
                {
                    ModelState.AddModelError("", "This email is already in use!");
                    return View(model);
                }

                try
                {
                    // 1. Sinh ngẫu nhiên mã OTP gồm 6 chữ số
                    Random rand = new Random();
                    string otpCode = rand.Next(100000, 999999).ToString();

                    // 2. Gửi mã OTP về Gmail của người đăng ký
                    SendVerificationEmail(model.Email, otpCode);

                    // 3. Lưu giữ tạm thời dữ liệu Form và mã OTP vào Session để chờ xác minh
                    Session["TempRegisterModel"] = model;
                    Session["GeneratedOTP"] = otpCode;

                    // 4. Chuyển hướng sang trang nhập mã OTP
                    return RedirectToAction("VerifyOTP");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to send verification email: " + ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        // ==========================================
        // CHỨC NĂNG XÁC MINH OTP (EMAIL VERIFICATION)
        // ==========================================

        // GET: Account/VerifyOTP
        public ActionResult VerifyOTP()
        {
            // Nếu không có dữ liệu đăng ký tạm thì đá về trang Register
            if (Session["TempRegisterModel"] == null || Session["GeneratedOTP"] == null)
            {
                return RedirectToAction("Register");
            }
            return View();
        }

        // POST: Account/VerifyOTP (BƯỚC 2: Kiểm tra OTP và lưu DB chính thức)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOTP(string enteredOTP)
        {
            var model = Session["TempRegisterModel"] as RegisterViewModel;
            string correctOTP = Session["GeneratedOTP"]?.ToString();

            if (model == null || string.IsNullOrEmpty(correctOTP))
            {
                return RedirectToAction("Register");
            }

            // Đối chiếu mã người dùng nhập với mã hệ thống đã sinh ra
            if (enteredOTP == correctOTP)
            {
                try
                {
                    // NẾU ĐÚNG MÃ: Tiến hành lưu chính thức tài khoản vào cơ sở dữ liệu
                    UserAccount user = new UserAccount
                    {
                        email = model.Email,
                        passwordHash = model.Password,
                        role = model.Role
                    };
                    db.UserAccounts.InsertOnSubmit(user);
                    db.SubmitChanges(); // Lưu để SQL tự cấp UserID tự tăng

                    // Lưu tiếp thông tin vào bảng chi tiết tương ứng dựa theo Role
                    if (model.Role == "Student")
                    {
                        Student student = new Student
                        {
                            userID = user.UserID,
                            fullName = model.FullName
                        };
                        db.Students.InsertOnSubmit(student);
                    }
                    else if (model.Role == "Teacher")
                    {
                        Teacher teacher = new Teacher
                        {
                            userID = user.UserID,
                            fullName = model.FullName
                        };
                        db.Teachers.InsertOnSubmit(teacher);
                    }

                    db.SubmitChanges(); // Hoàn tất lưu database

                    // Xóa sạch các Session lưu tạm để giải phóng bộ nhớ
                    Session.Remove("TempRegisterModel");
                    Session.Remove("GeneratedOTP");

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating your account: " + ex.Message);
                    return View();
                }
            }

            // NẾU SAI MÃ: Báo lỗi trực tiếp ra giao diện nhập OTP
            ModelState.AddModelError("", "Incorrect verification code. Please check your email again.");
            return View();
        }

        // ==========================================
        // CHỨC NĂNG ĐĂNG NHẬP (LOGIN)
        // ==========================================

        // GET: Login
        public ActionResult Login() => View();

        // POST: Login (BƯỚC 3: Đồng bộ kiểm tra Role từ View và nạp FullName động vào Session)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string SelectedRole)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra đồng thời email, passwordHash VÀ trường role trong database phải khớp với SelectedRole người dùng chọn
                var user = db.UserAccounts.FirstOrDefault(u => u.email == model.Email
                                                            && u.passwordHash == model.Password
                                                            && u.role == SelectedRole);
                if (user != null)
                {
                    // Lưu các Session dùng chung cho hệ thống
                    Session["UserId"] = user.UserID;
                    Session["UserEmail"] = user.email;
                    Session["Role"] = user.role;

                    // Rẽ nhánh điều hướng, cấp Session ID định danh riêng và lưu FullName động
                    if (user.role == "Student")
                    {
                        var student = db.Students.FirstOrDefault(s => s.userID == user.UserID);
                        if (student != null)
                        {
                            Session["StudentId"] = student.StudentID;
                            Session["FullName"] = student.fullName; // Hiển thị tên thật Student lên Navbar
                        }
                        return RedirectToAction("MyProfile", "Student");
                    }
                    else if (user.role == "Teacher")
                    {
                        var teacher = db.Teachers.FirstOrDefault(t => t.userID == user.UserID);
                        if (teacher != null)
                        {
                            Session["TeacherId"] = teacher.TeacherID;
                            Session["FullName"] = teacher.fullName; // Hiển thị tên thật Teacher lên Navbar
                        }
                        return RedirectToAction("Index", "Teacher");
                    }
                }

                ModelState.AddModelError("", "Invalid email, password, or incorrect role selected.");
            }
            return View(model);
        }

        // ==========================================
        // CHỨC NĂNG ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}