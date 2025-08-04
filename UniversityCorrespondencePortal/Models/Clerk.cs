    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static System.Web.Razor.Parser.SyntaxConstants;

    namespace UniversityCorrespondencePortal.Models
    {
        public class Clerk


        {
            public Clerk()
            {
                IsActive = true; // 👈 default value
            }


            [Key]
            [MaxLength(10)]
            public string ClerkID { get; set; }

        
            [MaxLength(100)]
            public string Name { get; set; }

            [MaxLength(100)]
            [EmailAddress]
            public string Email { get; set; }
        
            [MaxLength(200)]
            public string PasswordHash { get; set; }

            [MaxLength(15)]
            public string Phone { get; set; }

            public bool IsActive { get; set; }


            [Required]
            [MaxLength(10)]
            public string DepartmentID { get; set; }

            [ForeignKey("DepartmentID")]
            public virtual Department Department { get; set; }
        }
    }