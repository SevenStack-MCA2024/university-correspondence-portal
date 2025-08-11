using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniversityCorrespondencePortal.Models
{
    public class Department
    {
        public Department()
        {
            IsActive = true;
        }

        [Key]
        [Required(ErrorMessage = "Department ID is required")]
        [MaxLength(10, ErrorMessage = "Department ID cannot exceed 10 characters")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Department ID can only contain letters and numbers")]
        public string DepartmentID { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Department name can only contain letters and spaces")]
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "Department code is required")]
        [MaxLength(10, ErrorMessage = "Department code cannot exceed 10 characters")]
        [RegularExpression(@"^[A-Z]{2,10}$", ErrorMessage = "Only uppercase letters allowed")]
        public string DepartmentCode { get; set; }

        public bool IsActive { get; set; }

        // Navigation
        public virtual ICollection<Clerk> Clerks { get; set; }
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }
    }
}
