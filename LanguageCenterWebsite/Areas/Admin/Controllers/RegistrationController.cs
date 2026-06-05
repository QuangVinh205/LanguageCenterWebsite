using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using System.Linq;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class RegistrationController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();

        // ==========================
        // Registration List
        // ==========================
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

        // ==========================
        // Details
        // ==========================
        public ActionResult Details(int id)
        {
            var registration =
                db.Registrations
                  .SingleOrDefault(r => r.RegistrationID == id);

            if (registration == null)
            {
                return HttpNotFound();
            }

            return View(registration);
        }

        // ==========================
        // Update Status
        // ==========================
        public ActionResult UpdateStatus(int id)
        {
            var registration =
                db.Registrations
                  .SingleOrDefault(r => r.RegistrationID == id);

            if (registration == null)
            {
                return HttpNotFound();
            }

            return View(registration);
        }

        [HttpPost]
        public ActionResult UpdateStatus(Registration model)
        {
            var registration =
                db.Registrations
                  .SingleOrDefault(r =>
                      r.RegistrationID == model.RegistrationID);

            if (registration == null)
            {
                return HttpNotFound();
            }

            registration.status = model.status;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // ==========================
        // Cancel Registration
        // ==========================
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
            var registration =
                db.Registrations
                  .SingleOrDefault(r =>
                      r.RegistrationID == id);

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