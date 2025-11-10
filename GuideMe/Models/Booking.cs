using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{

  public  enum BookingStatus {
    
    Pending,
    Accepted,
    Rejected,
    Canceled
    
    }


    public class Booking
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public BookingStatus BookingStatus { get; set; }

        [Required]
        [Range(0.01,double.MaxValue)]
        public decimal BookingPrice { get; set; }

        public string? StripeSessionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public int TripId { get; set; }
        public Trip Trip { get; set; }

        [Required]
        public int GuideId { get; set; }
        public Guide Guide { get; set; }

        [Required]
        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; }

        public Payment Payment { get; set; }

        public ContactAccess ContactAccess { get; set; }
    }
}
