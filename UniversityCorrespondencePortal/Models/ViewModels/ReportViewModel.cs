using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class ReportViewModel
    {

        public string ReportType { get; set; }
        public List<InwardLetterViewModel> InwardLetters { get; set; }
        public List<OutwardLetterReportViewModel> OutwardLetters { get; set; } // Changed this line
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}