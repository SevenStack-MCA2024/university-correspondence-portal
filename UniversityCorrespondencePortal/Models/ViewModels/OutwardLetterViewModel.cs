using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UniversityCorrespondencePortal.Models;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class OutwardLetterViewModel
    {
        public int? OutwardLetterID { get; set; }

        [Required]
        [MaxLength(50)]
        public string LetterNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateReceived { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? TimeReceived { get; set; }

        [MaxLength(50)]
        public string DeliveryMode { get; set; }

        [MaxLength(100)]
        public string ReferenceID { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; }

        [MaxLength(100)]
        public string SenderDepartment { get; set; }

        [MaxLength(1000)]
        public string ReceiverDepartments { get; set; }

        [MaxLength(1000)]
        public string ReceiverNames { get; set; }

        public int? StaffID { get; set; }

        // Optional for displaying lists
        public List<Staff> AvailableStaff { get; set; }
        public List<Department> AllDepartments { get; set; }
    }
}
