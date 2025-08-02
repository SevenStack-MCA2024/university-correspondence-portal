using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices.CompensatingResourceManager;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class Department
    {
        [Key]
        [MaxLength(10)]
        public string DepartmentID { get; set; } // e.g., "100G"

     [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        // Navigation
        public virtual ICollection<Clerk> Clerks { get; set; }
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; }
        public virtual ICollection<LetterSerialTracker> LetterSerialTrackers { get; set; }
    }
}