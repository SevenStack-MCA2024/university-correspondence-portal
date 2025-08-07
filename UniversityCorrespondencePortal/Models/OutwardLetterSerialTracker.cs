using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class OutwardLetterSerialTracker
    {
        [Key]
        public int TrackerID { get; set; }

        [Required, MaxLength(10)]
        public string DepartmentID { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string LastSerialNumber { get; set; }

        public int? LetterID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }

        [ForeignKey("LetterID")]
        public virtual OutwardLetter OutwardLetter { get; set; }
    }
}
