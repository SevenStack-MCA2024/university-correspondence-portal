using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class VerificationCode
    {
        // ✅ This is now BOTH the Primary Key and Foreign Key
        [Key, ForeignKey("InwardLetter")]
        public int LetterID { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } // 6-digit string

        public bool IsUsed { get; set; } = false;

        // ✅ Navigation to the principal entity
        public virtual InwardLetter InwardLetter { get; set; }
    }
}
