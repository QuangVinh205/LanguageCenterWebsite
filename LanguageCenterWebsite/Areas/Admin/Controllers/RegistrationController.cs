using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class RegistrationController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();
        // GET: Admin/Registration
        public ActionResult Index()
        {
            var registrationList =
                from r in db.Registrations
                join s in db.Students
                on r.studentID equals s.StudentID

                join c in db.Classes
                on r.classID equals c.ClassID

                select new RegistrationViewModel
                {
                    RegistrationID = r.RegistrationID,
                    StudentName = s.fullName,
                    ClassName = c.className,
                    RegistrationDate = r.registrationDate,
                    Status = r.status
                };

            return View(registrationList.ToList());
        }
        public ActionResult UpdateStatus(int id)
        {
            var registration =
                db.Registrations
                  .SingleOrDefault(r =>
                        r.RegistrationID == id);

            if (registration == null)
            {
                return HttpNotFound();
            }

            return View(registration);
        }
        [HttpPost]
        public ActionResult UpdateStatus(int id,string status)
        {
            var registration =
                db.Registrations
                  .SingleOrDefault(r =>
                        r.RegistrationID == id);

            if (registration == null)
            {
                return HttpNotFound();
            }

            registration.status = status;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Cancel(int id)
        {
            var registration =
                db.Registrations
                  .SingleOrDefault(r =>
                        r.RegistrationID == id);

            if (registration == null)
            {
                return HttpNotFound();
            }

            return View(registration);
        }
        [HttpPost, ActionName("Cancel")]
        public ActionResult CancelConfirmed(int id)
        {
            var registration = db.Registrations
                  .SingleOrDefault(r => r.RegistrationID == id);
            if (registration == null)
            {
                return HttpNotFound();
            }

            registration.status = "Cancelled";

            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}