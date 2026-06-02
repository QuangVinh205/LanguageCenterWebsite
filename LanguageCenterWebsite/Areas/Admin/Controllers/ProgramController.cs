using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class ProgramController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();

        // GET: Admin/Programs
        public ActionResult Index()
        {
            var programs = db.Programs
                             .Where(p => p.Status == "Active")
                             .ToList();

            return View(programs);
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Program model)
        {
            LanguageDbDataContext db = new LanguageDbDataContext();

            db.Programs.InsertOnSubmit(model);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {

            var program = db.Programs.SingleOrDefault(p => p.ProgramID == id);
            if (program == null)
            {
                return HttpNotFound();
            }
            return View(program);
        }

        [HttpPost]
        public ActionResult Edit(Program model)
        {
            LanguageDbDataContext db = new LanguageDbDataContext();

            var program = db.Programs
                .SingleOrDefault(p => p.ProgramID == model.ProgramID);

            if (program == null)
            {
                return HttpNotFound();
            }

            program.programName = model.programName;
            program.level = model.level;
            program.duration = model.duration;
            program.fee = model.fee;
            program.description = model.description;
            program.outputStandard = model.outputStandard;
            program.Status = model.Status;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id) 
        {
            var program = db.Programs.SingleOrDefault(p => p.ProgramID == id);
            if (program == null)
            {
                return HttpNotFound();
            }

            return View(program);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var program = db.Programs.SingleOrDefault(p => p.ProgramID == id);
            if (program == null)
            {
                return HttpNotFound();
            }

            program.Status = "Inactive";

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

    }
}