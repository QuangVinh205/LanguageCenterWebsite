using System.Collections.Generic;

namespace LanguageCenterWebsite.Models
{
    public class TeacherDashboardViewModel
    {
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public List<ScheduleViewModel> TeachingSchedule { get; set; }
        public List<string> RecentActivities { get; set; }
    }

    public class ScheduleViewModel
    {
        public string Day { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string ClassName { get; set; }
    }
}