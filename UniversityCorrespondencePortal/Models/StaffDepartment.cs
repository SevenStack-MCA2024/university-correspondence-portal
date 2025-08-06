using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class StaffDepartment
    {
        [Key, Column(Order = 0)]
        [Required(ErrorMessage = "StaffID is required")]
        public int StaffID { get; set; }

        [Key, Column(Order = 1)]
        [Required(ErrorMessage = "DepartmentID is required")]
        [MaxLength(10)]
        public string DepartmentID { get; set; }

        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }
    }
}
