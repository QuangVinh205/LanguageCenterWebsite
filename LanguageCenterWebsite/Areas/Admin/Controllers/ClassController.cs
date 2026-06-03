using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class ClassController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();
        // GET: Admin/Class
        public ActionResult Index()
        {
            var classList =
                from c in db.Classes
                join p in db.Programs
                on c.programID equals p.ProgramID

                join cs in db.ClassStatus
                on c.statusID equals cs.StatusID

                join t in db.Teachers
                on c.teacherID equals t.TeacherID
                into teacherGroup

                from tg in teacherGroup.DefaultIfEmpty()
                where c.statusID != 4   // Deleted

                select new ClassViewModels
                {
                    ClassID = c.ClassID,
                    ClassName = c.className,
                    ProgramName = p.programName,
                    TeacherName = tg == null ? "Not Assigned" : tg.fullName,
                    StatusName = cs.statusName,
                    Room = c.room,
                    MaxStudents = c.maxStudents
                };

            return View(classList.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "programName");
            ViewBag.TeacherID = new SelectList(db.Teachers, "TeacherID", "fullName");
            ViewBag.StatusID = new SelectList(db.ClassStatus, "StatusID", "statusName");
            return View();
        }
        [HttpPost]
        public ActionResult Create(Class model)
        {
            if (ModelState.IsValid)
            {
                db.Classes.InsertOnSubmit(model);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "programName", model.programID);
            ViewBag.TeacherID = new SelectList(db.Teachers, "TeacherID", "fullName", model.teacherID);
            ViewBag.StatusID = new SelectList(db.ClassStatus, "StatusID", "statusName", model.statusID);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var classToEdit = db.Classes.FirstOrDefault(c => c.ClassID == id);
            if (classToEdit == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "programName", classToEdit.programID);
            ViewBag.TeacherID = new SelectList(db.Teachers, "TeacherID", "fullName", classToEdit.teacherID);
            ViewBag.StatusID = new SelectList(db.ClassStatus, "StatusID", "statusName", classToEdit.statusID);
            return View(classToEdit);
        }
        [HttpPost]
        public ActionResult Edit(Class model)
        {
            var oldClass = db.Classes
                             .SingleOrDefault(c => c.ClassID == model.ClassID);

            if (oldClass == null)
            {
                return HttpNotFound();
            }

            oldClass.className = model.className;
            oldClass.programID = model.programID;
            oldClass.teacherID = model.teacherID;
            oldClass.statusID = model.statusID;
            oldClass.room = model.room;
            oldClass.maxStudents = model.maxStudents;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var classItem = db.Classes
                              .SingleOrDefault(c => c.ClassID == id);

            if (classItem == null)
            {
                return HttpNotFound();
            }

            return View(classItem);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var classItem = db.Classes
                              .SingleOrDefault(c => c.ClassID == id);

            if (classItem == null)
            {
                return HttpNotFound();
            }

            classItem.statusID = 4; // Deleted

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

    }
}