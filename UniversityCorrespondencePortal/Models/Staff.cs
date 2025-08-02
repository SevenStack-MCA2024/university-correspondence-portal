using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class Staff
    {
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

        // Navigation
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }
        public virtual ICollection<InwardLetter> InwardLetters { get; set; }
        public virtual ICollection<OutwardLetter> OutwardLetters { get; set; }
    }
}