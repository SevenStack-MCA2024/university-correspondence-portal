using System;
using System.Collections.Generic;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class StaffOutwardLetterViewModel
    {
        public string Type { get; set; } // Always "Outward"

        public int LetterID { get; set; }
        public string OutwardNumber { get; set; }
        public DateTime? DateSent { get; set; }
        public string DeliveryMode { get; set; }
        public string SenderDepartment { get; set; }
        public string SenderName { get; set; }
        public int? SenderStaffID { get; set; }
        public string ReceiverDepartment { get; set; }
        public string ReceiverName { get; set; }
        public string ReferenceID { get; set; }
        public string Subject { get; set; }
        public string Remarks { get; set; }
        public string Priority { get; set; }
    }
}
