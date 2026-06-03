using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LanguageCenterWebsite.Models;
namespace LanguageCenterWebsite.Controllers
{
    public class HomeController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        // 1/ View Home (Banner, Lớp mới, Giáo viên)
        public ActionResult Index()
        {
            ViewBag.NewClasses = db.Classes.OrderByDescending(c => c.ClassID).Take(4).ToList();
            ViewBag.Teachers = db.Teachers.Take(4).ToList();
            return View();
        }

        // 2/ View Program List (Tìm kiếm, Lọc)
        public ActionResult ProgramList(string searchString, string levelFilter)
        {
            var programs = db.Programs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                programs = programs.Where(p => p.programName.Contains(searchString));

            if (!string.IsNullOrEmpty(levelFilter))
                programs = programs.Where(p => p.level == levelFilter);

            return View(programs.ToList());
        }

        // 3/ View Program Detail (Chi tiết và Lớp liên quan)
        public ActionResult ProgramDetail(int id)
        {
            var program = db.Programs.FirstOrDefault(p => p.ProgramID == id);
            if (program == null) return HttpNotFound();

            ViewBag.RelatedClasses = db.Classes.Where(c => c.programID == id).ToList();
            return View(program);
        }
    }
}