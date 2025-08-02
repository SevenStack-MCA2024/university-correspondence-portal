using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class StaffDepartment
    {
        [Key, Column(Order = 0)]
        public int StaffID { get; set; }

     
        [Key, Column(Order = 1)]
        [MaxLength(10)]
        public string DepartmentID { get; set; }

        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department Department { get; set; }
    }
}