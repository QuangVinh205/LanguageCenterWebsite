using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LanguageCenterWebsite.Models
{
    public class ClassMaterialViewModel
    {
        public int MaterialID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tài liệu")]
        public string MaterialName { get; set; }

        public string Description { get; set; }
        public string DocumentPath { get; set; }
        public string UploadDate { get; set; }
    }
}