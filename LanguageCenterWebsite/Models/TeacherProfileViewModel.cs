using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Models
{
    public class TeacherProfileViewModel
    {
        public int TeacherID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Avatar { get; set; }

        public string Expertise { get; set; }

        public string Bio { get; set; }
    }
}