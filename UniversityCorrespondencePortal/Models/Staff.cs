using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [MaxLength(50)]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } // ✅ Make unique via Fluent API

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(200)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [MaxLength(15)]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone must be 10 to 15 digits")]
        public string Phone { get; set; } // ✅ Make unique via Fluent API

        public bool IsActive { get; set; }

        // Navigation
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }
        public virtual ICollection<LetterStaff> LetterStaffs { get; set; }
        public virtual ICollection<OutwardLetter> OutwardLetters { get; set; }
    }
}
