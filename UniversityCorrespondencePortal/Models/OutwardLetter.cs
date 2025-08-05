using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class OutwardLetter
    {
        [Key]
        public int OutwardLetterID { get; set; }

        [Required]
        [MaxLength(50)]
        public string OutwardNumber { get; set; }

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

        // ✅ Multiple receiver departments (comma-separated or semicolon-separated)
        [MaxLength(1000)]
        public string ReceiverDepartments { get; set; }

        // ✅ Multiple receiver names (comma-separated or semicolon-separated)
        [MaxLength(1000)]
        public string ReceiverNames { get; set; }

        // ✅ Foreign Key for Staff
        public int? StaffID { get; set; }

        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }

        // ✅ Foreign Key for Department
        public string DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }
    }
}