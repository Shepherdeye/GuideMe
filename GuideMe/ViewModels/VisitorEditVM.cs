namespace GuideMe.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class VisitorEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
       
        public string LastName { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]

        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
      
        public string Email { get; set; }
        [Required,DataType(DataType.Password)]
       

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public GenderType Gender { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters.")]
        public string Country { get; set; }

        public string? ProfileImage { get; set; }

        [Required(ErrorMessage = "Passport number is required.")]
        [StringLength(20, ErrorMessage = "Passport number cannot exceed 20 characters.")]
     
        public string Passport { get; set; }

        [Required(ErrorMessage = "Visitor status is required.")]
       
        public VisitorStatus visitorStatus { get; set; }
    }

}
