using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UniversityCorrespondencePortal.Models.ViewModels
{
    public class StaffProfileViewModel
    {
        public int StaffID { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }

        public List<string> Departments { get; set; }
    }
}