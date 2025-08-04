using System;
using System.Collections.Generic;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class InwardLetterViewModel
    {
        public int LetterID { get; set; }
        public string InwardNumber { get; set; }
        public string OutwardNumber { get; set; }
        public DateTime? DateReceived { get; set; }
        public TimeSpan? TimeReceived { get; set; }
        public string DeliveryMode { get; set; }
        public string SenderDepartment { get; set; }
        public string SenderName { get; set; }
        public string ReferenceID { get; set; }
        public string Subject { get; set; }
        public string Remarks { get; set; }
        public string Priority { get; set; }
        public string ReceiverDepartment { get; set; }

        // Multiple staff names (comma-separated for display)
        public string StaffNames { get; set; }

        // List of selected staff IDs (for binding forms)
        public List<int> SelectedStaffIDs { get; set; }
    }

}
