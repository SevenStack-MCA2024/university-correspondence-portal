using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class OutwardLetter
    {
        [Key]
        public int LetterID { get; set; }

        [MaxLength(50)]
        public string LetterNo { get; set; }

        [MaxLength(50)]
        public string OutwardNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? Time { get; set; }

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

        [MaxLength(100)]
        public string SenderName { get; set; }

        [MaxLength(100)]
        public string ReceiverName { get; set; }

        [MaxLength(100)]
        public string ReceiverDepartment { get; set; }

        [Required]
        [MaxLength(10)]
        public string DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }

        // Navigation property for assigned staff
        public virtual ICollection<OutwardLetterStaff> OutwardLetterStaffs { get; set; }
    }
}
