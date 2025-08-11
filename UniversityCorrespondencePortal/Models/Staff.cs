using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class Staff
    {
        public Staff()
        {
            IsActive = true; // Default to active
            MustResetPassword = false;
        }

        [Key]
        public int StaffID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [MaxLength(50, ErrorMessage = "Designation cannot exceed 50 characters")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } // Unique via Fluent API

        [MaxLength(200)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(10, ErrorMessage = "Phone cannot exceed 10 digits")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string Phone { get; set; } // Unique via Fluent API

        public bool IsActive { get; set; }

        public bool MustResetPassword { get; set; }

        // Navigation properties
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }
        public virtual ICollection<LetterStaff> LetterStaffs { get; set; }
        public virtual ICollection<OutwardLetterStaff> OutwardLetterStaffs { get; set; }
    }
}
