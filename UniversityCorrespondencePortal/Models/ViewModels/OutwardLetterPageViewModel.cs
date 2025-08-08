using System.Collections.Generic;

namespace UniversityCorrespondencePortal.ViewModels
{
    public class OutwardLetterPageViewModel
    {
        public OutwardLetterViewModel NewLetter { get; set; }
        public IEnumerable<UniversityCorrespondencePortal.Models.OutwardLetter> OutwardLetters { get; set; }
    }
}
