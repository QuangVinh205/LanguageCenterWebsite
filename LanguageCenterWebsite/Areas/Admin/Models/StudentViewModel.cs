using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Areas.Admin.Models
{
    public class StudentViewModel
    {
        public int StudentID { get; set; }

        public string fullName { get; set; }

        public string phone { get; set; }

        public string status { get; set; }
    }
}