using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityCorrespondencePortal.Models
{
    public class OutwardLetterStaff
    {
        public int LetterID { get; set; }
        public OutwardLetter OutwardLetter { get; set; }

        public int StaffID { get; set; }
        public Staff Staff { get; set; }
    }
}
