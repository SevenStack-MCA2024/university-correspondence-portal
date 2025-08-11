using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniversityCorrespondencePortal.Models
{
    public class InwardLetter
    {
        [Key]
        public int LetterID { get; set; }

        [Required(ErrorMessage = "Inward Number is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Inward Number must be between 3 and 50 characters")]
        [RegularExpression(@"^[A-Za-z0-9\-\/]+$", ErrorMessage = "Inward Number can only contain letters, numbers, hyphens, and slashes")]
        public string InwardNumber { get; set; }

        [StringLength(50, ErrorMessage = "Outward Number cannot exceed 50 characters")]
        [RegularExpression(@"^[A-Za-z0-9\-\/]*$", ErrorMessage = "Outward Number can only contain letters, numbers, hyphens, and slashes")]
        public string OutwardNumber { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(typeof(InwardLetter), nameof(ValidateNotFutureDate))]
        public DateTime? DateReceived { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? TimeReceived { get; set; }

        [Required(ErrorMessage = "Delivery Mode is required")]
        [StringLength(50, ErrorMessage = "Delivery Mode cannot exceed 50 characters")]
        public string DeliveryMode { get; set; }

        [StringLength(100, ErrorMessage = "Sender Department cannot exceed 100 characters")]
        public string SenderDepartment { get; set; }

        [Required(ErrorMessage = "Sender Name is required")]
        [StringLength(100, ErrorMessage = "Sender Name cannot exceed 100 characters")]
        public string SenderName { get; set; }

        [StringLength(100, ErrorMessage = "Reference ID cannot exceed 100 characters")]
        public string ReferenceID { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(255, ErrorMessage = "Subject cannot exceed 255 characters")]
        public string Subject { get; set; }

        [StringLength(255, ErrorMessage = "Remarks cannot exceed 255 characters")]
        public string Remarks { get; set; }

        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; }

        [StringLength(100, ErrorMessage = "Receiver Department cannot exceed 100 characters")]
        public string ReceiverDepartment { get; set; }

        public virtual ICollection<LetterStaff> LetterStaffs { get; set; }

        // Custom validation method to ensure date is not in the future
        public static ValidationResult ValidateNotFutureDate(DateTime? date, ValidationContext context)
        {
            if (date.HasValue && date.Value.Date > DateTime.Today)
            {
                return new ValidationResult("Date received cannot be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
