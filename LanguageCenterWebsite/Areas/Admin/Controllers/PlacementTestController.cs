using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using PlacementTestViewModel = LanguageCenterWebsite.Areas.Admin.Models.PlacementTestViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class PlacementTestController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();
        // GET: Admin/PlacementTest
        public ActionResult Index()
        {
            var testList =
                from t in db.PlacementTests
                join s in db.Students
                on t.studentID equals s.StudentID

                select new PlacementTestViewModel
                {
                    TestID = t.TestID,
                    StudentName = s.fullName,
                    TestDate = t.testDate,
                    TestTime = t.testTime,
                    LevelRequested = t.levelRequested,
                    SuggestedLevel = t.suggestedLevel,
                    ResultScore = t.resultScore,
                    Status = t.status
                };

            return View(testList.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.studentID = new SelectList(db.Students, "StudentID", "fullName");
            return View();
        }
        [HttpPost]
        public ActionResult Create(PlacementTest model)
        {
            if (ModelState.IsValid)
            {
                model.status = "Scheduled";

                db.PlacementTests.InsertOnSubmit(model);

                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            ViewBag.StudentID =
                new SelectList(db.Students,"StudentID","fullName",model.studentID);
            return View(model);
        }

        public ActionResult UpdateResult(int id)
        {
            var test =
                db.PlacementTests
                  .SingleOrDefault(
                    t => t.TestID == id);

            if (test == null)
            {
                return HttpNotFound();
            }

            return View(test);
        }
        [HttpPost]
        public ActionResult UpdateResult(PlacementTest model)
        {
            var oldTest =db.PlacementTests.SingleOrDefault(t => t.TestID == model.TestID);
            if (oldTest == null)
            {
                return HttpNotFound();
            }
            oldTest.resultScore = model.resultScore;
            oldTest.suggestedLevel = model.suggestedLevel;
            oldTest.status = "Completed";
            db.SubmitChanges();
            return RedirectToAction("Index");
        }
    }
}