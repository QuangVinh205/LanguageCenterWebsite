using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Models
{
    public class PlacementTestViewModel
    {
        public int TestID { get; set; }
        public string StudentName { get; set; }     // Tên học viên
        public string TestDate { get; set; }        // Ngày thi (Dạng chuỗi dd/MM/yyyy)
        public string SuggestedLevel { get; set; }  // Lớp đề xuất
        public double ResultScore { get; set; }     // Điểm số kết quả
        public string Status { get; set; }         // Trạng thái (Completed/Pending...)
    }
}