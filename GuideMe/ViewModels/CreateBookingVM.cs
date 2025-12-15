using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class CreateBookingVM
    {
        [Required]
        public int OfferId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal BookingPrice { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int TripId { get; set; }
        [Required]
        public int GuideId { get; set; }
        [Required]
        public int VisitorId { get; set; }

    }
}
