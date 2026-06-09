using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Models
{
    public class ClassMaterial
    {
        public string MaterialID { get; set; }
        public int ClassID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadDate { get; set; }
        public string FileSize { get; set; }
    }
}