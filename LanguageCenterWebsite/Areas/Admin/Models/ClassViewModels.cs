using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Areas.Admin.Models
{
    public class ClassViewModels
    {
        public int ClassID { get; set; }

        public string ClassName { get; set; }

        public string ProgramName { get; set; }

        public string TeacherName { get; set; }

        public string StatusName { get; set; }

        public string Room { get; set; }

        public int? MaxStudents { get; set; }
    }
}