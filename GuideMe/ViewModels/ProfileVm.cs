using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class ProfileVm
    {
        public string Id { get; set; }

        [Required, MinLength(5), MaxLength(20)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MinLength(5), MaxLength(20)]
        public string LastName { get; set; } = string.Empty;

        [Required,MinLength(5),MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required,DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }

        [Required, MinLength(5), MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required, MinLength(11), MaxLength(14)]
        public string PhoneNumber { get; set; } = string.Empty;

        public UserRole? Role { get; set; }


    }
}
