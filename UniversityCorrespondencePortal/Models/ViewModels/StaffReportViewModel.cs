using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class StaffReportViewModel
    {
        public int TotalInward { get; set; }
        public int TotalOutward { get; set; }

        public List<YearlyLetterStats> YearlyStats { get; set; }
    }

    public class YearlyLetterStats
    {
        public int Year { get; set; }
        public int InwardCount { get; set; }
        public int OutwardCount { get; set; }
    }
}