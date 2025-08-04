using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class InwardLetter
    {
        [Key]
        public int LetterID { get; set; }

        [Required]
        [MaxLength(50)]
        public string InwardNumber { get; set; }

        [MaxLength(50)]
        public string OutwardNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateReceived { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? TimeReceived { get; set; }

        [MaxLength(50)]
        public string DeliveryMode { get; set; }

        [MaxLength(100)]
        public string SenderDepartment { get; set; }

        [MaxLength(100)]
        public string SenderName { get; set; }

        [MaxLength(100)]
        public string ReferenceID { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; }

        [MaxLength(100)]
        public string ReceiverDepartment { get; set; }

        // Remove StaffID
        // public int? StaffID { get; set; }
        // public virtual Staff Staff { get; set; }

        // Many-to-many navigation
        public virtual ICollection<LetterStaff> LetterStaffs { get; set; }
    }
}
