using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class OutwardLetterReportViewModel
    {
        public string LetterNo { get; set; }
        public DateTime? Date { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverDepartment { get; set; }
        public string Subject { get; set; }
    }
}