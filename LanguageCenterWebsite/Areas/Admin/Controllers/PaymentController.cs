using LanguageCenterWebsite.Areas.Admin.Models;
using LanguageCenterWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenterWebsite.Areas.Admin.Controllers
{
    public class PaymentController : Controller
    {
        LanguageDbDataContext db = new LanguageDbDataContext();
        // GET: Admin/Payment
        public ActionResult Index()
        {
            var paymentList =
                from p in db.Payments

                join r in db.Registrations
                on p.registrationID equals r.RegistrationID

                join s in db.Students
                on r.studentID equals s.StudentID

                join c in db.Classes
                on r.classID equals c.ClassID

                select new PaymentViewModel
                {
                    PaymentID = p.PaymentID,
                    StudentName = s.fullName,
                    ClassName = c.className,
                    amount = p.amount,
                    paymentDate = p.paymentDate,
                    paymentMethod = p.paymentMethod,
                    paymentStatus = p.paymentStatus
                };

            return View(paymentList.ToList());
        }
        public ActionResult Confirm(int id)
        {
            var payment =
                db.Payments
                  .SingleOrDefault(p => p.PaymentID == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }
        [HttpPost, ActionName("Confirm")]
        public ActionResult ConfirmConfirmed(int id)
        {
            var payment =
                db.Payments
                  .SingleOrDefault(p => p.PaymentID == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            payment.paymentStatus = "Paid";

            payment.paymentDate = DateTime.Now;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }
        public ActionResult UpdateStatus(int id)
        {
            var payment =
                db.Payments
                  .SingleOrDefault(p => p.PaymentID == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }
        [HttpPost]
        public ActionResult UpdateStatus(int id,string paymentStatus)
        {
            var payment = db.Payments.SingleOrDefault(p => p.PaymentID == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            payment.paymentStatus = paymentStatus;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }

}