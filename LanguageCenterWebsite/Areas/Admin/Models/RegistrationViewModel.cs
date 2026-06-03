using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Areas.Admin.Models
{
    public class RegistrationViewModel
    {
        public int RegistrationID { get; set; }

        public string StudentName { get; set; }

        public string ClassName { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public string Status { get; set; }
    }
}