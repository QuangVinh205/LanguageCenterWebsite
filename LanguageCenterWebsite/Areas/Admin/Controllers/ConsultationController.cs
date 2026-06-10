using System.Linq;
using System.Web.Mvc;
using LanguageCenterWebsite.Models;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class ConsultationController : Controller
    {
        private LanguageDbDataContext db = new LanguageDbDataContext();

        public ActionResult Index()
        {
            var consultations = db.Consultations
                .OrderByDescending(c => c.ConsultationID)
                .ToList();

            return View(consultations);
        }

        public ActionResult MarkDone(int id)
        {
            var consultation = db.Consultations
                .FirstOrDefault(c => c.ConsultationID == id);

            if (consultation == null)
                return HttpNotFound();

            consultation.requestStatus = "Done";
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult MarkPending(int id)
        {
            var consultation = db.Consultations
                .FirstOrDefault(c => c.ConsultationID == id);

            if (consultation == null)
                return HttpNotFound();

            consultation.requestStatus = "Pending";
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}