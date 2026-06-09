using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenterWebsite.Models
{
    public class StudentFeedbackViewModel
    {
        internal int ConsultationID;

        public int FeedbackID { get; set; }
        public string StudentName { get; set; }     // Tên học viên / Khách hàng đóng góp
        public string FeedbackContent { get; set; } // Nội dung phản hồi (Lấy từ cột question)
        public int Rating { get; set; }             // Đánh giá sao (Mặc định mồi dữ liệu 4-5 sao)
        public DateTime FeedbackDate { get; set; }    // Ngày phản hồi (Dạng chuỗi dd/MM/yyyy)
        public string Status { get; set; }           // Trạng thái xử lý (Pending/Contacted)
    }
}