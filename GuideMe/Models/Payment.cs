using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        [Required]
        [Range(0.01,double.MaxValue)]
        public Decimal Amount { get; set; }


        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public decimal ServiceFeeGuide { get; set; }
        [Required]
        public decimal ServiceFeeVisitor { get; set; }

        

    }
}
