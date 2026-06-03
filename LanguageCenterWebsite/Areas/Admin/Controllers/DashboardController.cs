using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.TotalPrograms = db.Programs.Count();

            ViewBag.TotalClasses = db.Classes.Count();

            ViewBag.ActiveClasses =
                db.Classes.Count(c => c.statusID == 1);

            ViewBag.ClosedClasses =
                db.Classes.Count(c => c.statusID == 3);

            ViewBag.DeletedClasses =
                db.Classes.Count(c => c.statusID == 4);

            ViewBag.TotalStudents = db.Students.Count();

            ViewBag.ActiveStudents =
                db.UserAccounts.Count(u => u.role == "Student" && u.status == "Active");

            ViewBag.InactiveStudents =
                db.UserAccounts.Count(u => u.role == "Student" && u.status == "Deactivate");

            ViewBag.TotalTeachers = db.Teachers.Count();

            ViewBag.ActiveTeachers =
                db.UserAccounts.Count(u => u.role == "Teacher" && u.status == "Active");

            ViewBag.InactiveTeachers =
                db.UserAccounts.Count(u => u.role == "Teacher" && u.status == "Deactivate");

            ViewBag.TotalRevenue =
                db.Payments
                  .Where(p => p.paymentStatus == "Paid")
                  .Sum(p => (decimal?)p.amount) ?? 0;

            return View();
        }
    }
}