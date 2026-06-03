using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class StudentController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();
        // GET: Admin/Student
        public ActionResult Index()
        {
            var studentList = from s in db.Students
                              join u in db.UserAccounts
                              on s.userID equals u.UserID
                              select new StudentViewModel
                              {
                                  StudentID = s.StudentID,
                                  fullName = s.fullName,
                                  phone = s.phone,
                                  status = u.status
                              };
            return View(studentList);
        }

        public ActionResult Edit(int id)
        {
            var student = db.Students.SingleOrDefault(s => s.StudentID == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }
        [HttpPost]
        public ActionResult Edit(int id, string fullName, string phone)
        {
            var oldstudent = db.Students.SingleOrDefault(s => s.StudentID == id);
            if (oldstudent == null)
            {
                return HttpNotFound();
            }
            oldstudent.fullName = fullName;
            oldstudent.phone = phone;
            oldstudent.avatar = oldstudent.avatar;
            oldstudent.dateOfBirth = oldstudent.dateOfBirth;

            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Deactivate(int id)
        {
            var student = db.Students.SingleOrDefault(s => s.StudentID == id);

            if (student == null)
            {
                return HttpNotFound();
            }

            return View(student);
        }
        [HttpPost, ActionName("Deactivate")]
        public ActionResult DeactivateConfirmed(int id)
        {
            var student = db.Students
                            .SingleOrDefault(s => s.StudentID == id);

            var user = db.UserAccounts
                         .SingleOrDefault(u => u.UserID == student.userID);

            if (user != null)
            {
                user.status = "Inactive";
                db.SubmitChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Activate(int id)
        {
            var student = db.Students.SingleOrDefault(s => s.StudentID == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }
        [HttpPost, ActionName("Activate")]
        public ActionResult ActivateConfirmed(int id)
        {
            var student = db.Students
                            .SingleOrDefault(s => s.StudentID == id);

            var user = db.UserAccounts
                         .SingleOrDefault(u => u.UserID == student.userID);

            if (user != null)
            {
                user.status = "Active";
                db.SubmitChanges();
            }

            return RedirectToAction("Index");
        }
    }
}