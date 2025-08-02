using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class LetterSerialTracker
    {
        [Key]
        public int TrackerID { get; set; }

   
        [Required]
        [MaxLength(10)]
        public string DepartmentID { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string LastSerialNumber { get; set; }

        public int? LetterID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }

        [ForeignKey("LetterID")]
        public virtual InwardLetter InwardLetter { get; set; }
    }
}