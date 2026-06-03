using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Areas.Admin.Models
{
    public class PaymentViewModel
    {
        public int PaymentID { get; set; }

        public string StudentName { get; set; }

        public string ClassName { get; set; }

        public decimal amount { get; set; }

        public DateTime? paymentDate { get; set; }

        public string paymentMethod { get; set; }

        public string paymentStatus { get; set; }
    }
}