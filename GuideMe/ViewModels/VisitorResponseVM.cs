namespace GuideMe.ViewModels
{
    public class VisitorResponseVM
    {
        public int Id { get; set; }
        public string FirstName{ get; set; }

        public string LastName{ get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public GenderType Gender { get; set; }

        public string  Country { get; set; }

        public UserRole Role { get; set; }

        public string ProfileImage { get; set; }

        public string Passport { get; set; }

        public VisitorStatus visitorStatus { get; set; }


    }
}
