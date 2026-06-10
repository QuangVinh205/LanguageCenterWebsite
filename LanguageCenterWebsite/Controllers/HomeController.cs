using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using LanguageCenterWebsite.Models;
namespace LanguageCenterWebsite.Controllers
{
    public class HomeController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        // 1/ View Home (Banner, Lớp mới, Giáo viên)
        public ActionResult Index()
        {
            ViewBag.NewClasses = db.Classes
                .Where(c => c.statusID == 1)
                .Take(4)
                .ToList();

            ViewBag.Teachers = db.Teachers.Take(4).ToList();

            ViewBag.Programs = db.Programs.Take(3).ToList();

            return View();
        }

        // 2/ View Program List (Tìm kiếm, Lọc) Pagination
        public ActionResult ProgramList(string searchString, string levelFilter, int? page)
        {
            var programs = db.Programs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                programs = programs.Where(
                    p => p.programName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(levelFilter))
            {
                programs = programs.Where(
                    p => p.level == levelFilter);
            }

            int pageSize = 6;
            int pageNumber = page ?? 1;

            return View(
                programs
                .OrderBy(p => p.ProgramID)
                .ToPagedList(pageNumber, pageSize)
            );
        }

        // 3/ View Program Detail (Chi tiết và Lớp liên quan)
        public ActionResult ProgramDetail(int id)
        {
            var program = db.Programs.FirstOrDefault(p => p.ProgramID == id);
            if (program == null) return HttpNotFound();

            ViewBag.RelatedClasses = db.Classes.Where(c => c.programID == id).ToList();
            return View(program);
        }

        public ActionResult ClassList(string searchString, int? page)
        {
            var classes = db.Classes
                .Where(c => c.statusID == 1)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(c =>
                    c.className.Contains(searchString));
            }

            int pageSize = 8;
            int pageNumber = page ?? 1;

            return View(
                classes
                .OrderBy(c => c.ClassID)
                .ToPagedList(pageNumber, pageSize)
            );
        }

        public ActionResult ClassDetail(int id)
        {
            var cls = db.Classes
                        .FirstOrDefault(c => c.ClassID == id);

            if (cls == null)
                return HttpNotFound();

            return View(cls);
        }
    }
}