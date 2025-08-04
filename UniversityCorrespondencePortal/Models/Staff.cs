using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class Staff
    {

        public Staff()
        {
            IsActive = true; // 👈 default value
        }
        [Key]
        public int StaffID { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Designation { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(200)]
        public string PasswordHash { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        // ✅ New field
        public bool IsActive { get; set; }


        // Navigation
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }

        // Remove direct reference to InwardLetters
        // public virtual ICollection<InwardLetter> InwardLetters { get; set; }

        public virtual ICollection<LetterStaff> LetterStaffs { get; set; }
        public virtual ICollection<OutwardLetter> OutwardLetters { get; set; }
    }

}