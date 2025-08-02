using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Web.Razor.Parser.SyntaxConstants;

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

        [Required]
        public DateTime DateTimeSent { get; set; }

        [MaxLength(50)]
        public string DeliveryMode { get; set; }

        [MaxLength(100)]
        public string ReceiverDepartment { get; set; }

        [MaxLength(100)]
        public string ReceiverName { get; set; }

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
        public string SenderDepartment { get; set; }

        [MaxLength(10)]
        public string ClerkID { get; set; }

        public int? StaffID { get; set; }

        [ForeignKey("ClerkID")]
        public virtual Clerk Clerk { get; set; }

        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }
    }
}