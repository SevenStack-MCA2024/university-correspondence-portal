using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace UniversityCorrespondencePortal.Models
{
    public class Admin
    {
        [Key]
        [Required]
        [MaxLength(10)]
        public string AdminID { get; set; } // e.g., ADM001, SUP100

  
      [Required]
      [MaxLength(100)]
      public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } = "SuperAdmin";
    }
}