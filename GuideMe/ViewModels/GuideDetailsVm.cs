using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class GuideDetailsVm
    {
        public int Id { get; set; }
        [Required]
        public int YearsOfExperience { get; set; }
        [Required]
        public string NationalId { get; set; }=string.Empty;
        [Required]
        public string LicenseNumber { get; set; } = string.Empty;
    }
}
