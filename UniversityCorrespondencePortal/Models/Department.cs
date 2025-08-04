using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UniversityCorrespondencePortal.Models;
namespace UniversityCorrespondencePortal.Models
{

    public class Department
    {

        public Department()
        {
            IsActive = true; // 👈 default value
        }
        [Key]
        [MaxLength(10)]
        public string DepartmentID { get; set; } // e.g., "100G"

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        [MaxLength(10)]
        public string DepartmentCode { get; set; } // NEW: e.g., SOCS, SOSS, CSE

        // ✅ New field
        public bool IsActive { get; set; }


        // Navigation
        public virtual ICollection<Clerk> Clerks { get; set; }
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }
        public virtual ICollection<LetterSerialTracker> LetterSerialTrackers { get; set; }
    }
}