using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Areas.Admin.Models
{
    public class PlacementTestViewModel
    {
        public int TestID { get; set; }

        public string StudentName { get; set; }

        public DateTime? TestDate { get; set; }

        public TimeSpan? TestTime { get; set; }

        public string LevelRequested { get; set; }

        public string SuggestedLevel { get; set; }

        public decimal? ResultScore { get; set; }

        public string Status { get; set; }
    }
}