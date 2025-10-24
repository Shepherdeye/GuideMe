using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{

    public enum VisitorStatus {

      Available,
      NotAvailable

    }

    public class Visitor
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Passport { get; set; } = string.Empty;
        [Required]
        public VisitorStatus visitorStatus { get; set; }
        [Required]
        
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();

    }
}
