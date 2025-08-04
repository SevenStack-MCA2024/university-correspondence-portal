using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UniversityCorrespondencePortal.Models
{
    public class LetterStaff
    {
        public int LetterID { get; set; }
        public InwardLetter InwardLetter { get; set; }

        public int StaffID { get; set; }
        public Staff Staff { get; set; }
    }
}