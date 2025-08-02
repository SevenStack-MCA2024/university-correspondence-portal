using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Web.Razor.Parser.SyntaxConstants;

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

        public DateTime? DateTimeReceived { get; set; }

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

        [MaxLength(50)]
        public string Status { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; }

        [MaxLength(100)]
        public string ReceiverDepartment { get; set; }

        public int? StaffID { get; set; }

        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }

        public virtual VerificationCode VerificationCode { get; set; }
    }
}