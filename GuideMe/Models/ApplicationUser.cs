using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
   public enum UserRole
    {
        Visitor,
        Guide
    }

    public enum GenderType
    {
        Male,
        Female
    }

    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
  
        [Required]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$",
        ErrorMessage = "Please enter a valid Egyptian phone number (e.g. 01012345678)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }
        [Required]
        public GenderType Gender { get; set; }

        public Visitor? Visitor { get; set; }
        public Guide? Guide { get; set; }

    }
}
