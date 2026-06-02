using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Areas.Admin.Models
{
    public class TeacherViewModel
    {
        public int TeacherID { get; set; }

        public string fullName { get; set; }

        public string expertise { get; set; }

        public string status { get; set; }

    }
}