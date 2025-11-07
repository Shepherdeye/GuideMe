using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class GuideRegisterVm
    {

        public int id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; }


        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required, DataType(DataType.Password)]
        public string Password { get; set; }


        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }


        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string LicenseNumber { get; set; } = string.Empty;
        [Required]
        public int YearsOfExperience { get; set; }
        [Required]
        public string NationalId { get; set; } = string.Empty;
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public GenderType Gender { get; set; }

    }
}
