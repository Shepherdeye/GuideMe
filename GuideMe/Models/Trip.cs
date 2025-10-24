using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuideMe.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50, ErrorMessage = "Maxmum length 50 letter")
            , MinLength(10, ErrorMessage = "min length 10")]
        public string Title { get; set; } = string.Empty;
        [Required]
        [MinLength(10, ErrorMessage = "min length 10")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [MinLength(10, ErrorMessage = "min length 10")]
        public string Destination { get; set; } = string.Empty;
        [Required]
        [MinLength(10, ErrorMessage = "min length 10")]
        public string Region { get; set; } =  string.Empty;
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        public int Members { get; set; }
        [Required]
       
        public int NumberOfDays { get; set; }
        [Required]
        public string Image { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedOn { get; set; }
        [Required]
        public DateTime LastUpdatedon { get; set; }

        public int VisitorId { get; set; }

        public Visitor Visitor { get; set; }

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();

    }
}
