using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class TeacherController : Controller
    {
        // GET: Admin/Teacher
        LanguageDbDataContext db = new LanguageDbDataContext();
        public ActionResult Index()
        {
            var teacherList = from t in db.Teachers
                              join u in db.UserAccounts
                              on t.userID equals u.UserID
                              select new TeacherViewModel
                              {
                                  TeacherID = t.TeacherID,
                                  fullName = t.fullName,
                                  expertise = t.expertise,
                                  status = u.status
                              };
            return View(teacherList.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(string email,
                                    string password,
                                    string fullName,
                                    string expertise,
                                    string bio)
        {
            // Kiểm tra email tồn tại
            var checkEmail = db.UserAccounts
                               .SingleOrDefault(u => u.email == email);

            if (checkEmail != null)
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            // Tạo tài khoản
            UserAccount user = new UserAccount();

            user.email = email;
            user.passwordHash = password;
            user.role = "Teacher";
            user.status = "Active";

            db.UserAccounts.InsertOnSubmit(user);
            db.SubmitChanges();

            // Tạo giáo viên
            Teacher teacher = new Teacher();

            teacher.userID = user.UserID;
            teacher.fullName = fullName;
            teacher.expertise = expertise;
            teacher.bio = bio;

            db.Teachers.InsertOnSubmit(teacher);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var teacher = db.Teachers.SingleOrDefault(t => t.TeacherID == id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }
        [HttpPost]
        public ActionResult Edit(Teacher teacher)
        {
            var oldteacher = db.Teachers
                .SingleOrDefault(t => t.TeacherID == teacher.TeacherID);
            if (oldteacher == null)
            {
                return HttpNotFound();
            }
            oldteacher.fullName = teacher.fullName;
            oldteacher.expertise = teacher.expertise;
            oldteacher.bio = teacher.bio;
            oldteacher.avatar = teacher.avatar;
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Deactivate(int id)
        {
            var teacher = db.Teachers.SingleOrDefault(t => t.TeacherID == id);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            return View(teacher);
        }

        [HttpPost, ActionName("Deactivate")]
        public ActionResult DeactivateConfirmed(int id)
        {
            var teacher = db.Teachers
                            .SingleOrDefault(t => t.TeacherID == id);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            var user = db.UserAccounts
                         .SingleOrDefault(u => u.UserID == teacher.userID);

            if (user != null)
            {
                user.status = "Deactivate";
                db.SubmitChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Activate(int id)
        {
            var teacher = db.Teachers.SingleOrDefault(t => t.TeacherID == id);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            return View(teacher);
        }

        [HttpPost, ActionName("Activate")]
        public ActionResult ActivateConfirmed(int id)
        {
            var teacher = db.Teachers
                            .SingleOrDefault(t => t.TeacherID == id);

            if (teacher == null)
            {
                return HttpNotFound();
            }

            var user = db.UserAccounts
                         .SingleOrDefault(u => u.UserID == teacher.userID);

            if (user != null)
            {
                user.status = "Active";
                db.SubmitChanges();
            }

            return RedirectToAction("Index");
        }
    }
}