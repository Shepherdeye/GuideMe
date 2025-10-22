using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public class Guide
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string LicenseNumber { get; set; } = string.Empty;
        [Required]
        public int YearsOfExperience { get; set; }
        [Required]
        public string NationalId { get; set; } = string.Empty;
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();

    }
}
