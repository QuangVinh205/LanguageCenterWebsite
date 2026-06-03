using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static System.Collections.Specialized.BitVector32;

namespace LanguageCenterWebsite.Controllers
{
    public class AccountController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        // GET: Register
        public ActionResult Register() => View();

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.UserAccounts.Any(u => u.email == model.Email))
                {
                    ModelState.AddModelError("", "Email này đã tồn tại!");
                    return View(model);
                }

                // Lưu vào bảng UserAccount trước
                UserAccount user = new UserAccount { email = model.Email, passwordHash = model.Password, role = "Student" };
                db.UserAccounts.InsertOnSubmit(user);
                db.SubmitChanges();

                // Lưu tiếp vào bảng Student liên kết
                Student student = new Student { userID = user.UserID, fullName = model.FullName };
                db.Students.InsertOnSubmit(student);
                db.SubmitChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Login
        public ActionResult Login() => View();

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.UserAccounts.FirstOrDefault(u => u.email == model.Email && u.passwordHash == model.Password);
                if (user != null)
                {
                    Session["UserId"] = user.UserID;
                    Session["UserEmail"] = user.email;
                    Session["Role"] = user.role;

                    if (user.role == "Student")
                    {
                        var student = db.Students.FirstOrDefault(s => s.userID == user.UserID);
                        Session["StudentId"] = student.StudentID;
                        return RedirectToAction("MyProfile", "Student");
                    }
                }
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}

