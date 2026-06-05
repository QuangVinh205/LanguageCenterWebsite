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
            ViewBag.TotalStudents = db.Students.Count();
            ViewBag.TotalTeachers = db.Teachers.Count();
            ViewBag.TotalClasses = db.Classes.Count();
            ViewBag.TotalRegistrations = db.Registrations.Count();

            return View();
        }
    }
}