using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public class ContactAccess
    {
        public int Id { get; set; }


        [Required]
        public int BookingId { get; set; }
        public Booking  Booking { get; set; }
    }
}
