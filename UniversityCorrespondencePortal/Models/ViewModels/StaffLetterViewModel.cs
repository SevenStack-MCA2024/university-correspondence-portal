using System;
using System.Collections.Generic;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class StaffLetterViewModel
    {
        public string Type { get; set; } // Add this: "Inward" or "Outward"

        public int LetterID { get; set; }
        public string InwardNumber { get; set; }
        public string OutwardNumber { get; set; }
        public int TotalInward { get; set; }
        public int TotalOutward { get; set; }
        public DateTime? DateReceived { get; set; }
        public string DeliveryMode { get; set; }
        public string SenderDepartment { get; set; }
        public string SenderName { get; set; }
        public string ReferenceID { get; set; }
        public string Subject { get; set; }
        public string Remarks { get; set; }
        public string Priority { get; set; }
        public string ReceiverDepartment { get; set; }

        public List<string> AssignedStaffNames { get; set; }
    }
}