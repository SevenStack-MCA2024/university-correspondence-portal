using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class ClerkViewModel
    {
        public string ClerkID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DepartmentName { get; set; } // For display
        public bool IsActive { get; set; }
        public bool MustResetPassword { get; set; } = true;

    }
}