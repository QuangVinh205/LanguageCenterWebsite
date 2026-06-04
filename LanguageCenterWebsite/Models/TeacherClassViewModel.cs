namespace LanguageCenterWebsite.Models
{
    public class TeacherClassViewModel
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string Room { get; set; }
        public string Status { get; set; }
        public int StudentCount { get; set; }
        public string Schedule { get; set; }
    }
}