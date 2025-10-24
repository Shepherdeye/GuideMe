using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public enum RatingReview
    {
        VeryBad,
        Bad,
        Good,
        VeryGood,
        Excellent
    }


    public class Review
    {
        [Key]
        public int Id { get; set; }

        [MinLength(10,ErrorMessage ="Msg Should have atleast 10 letters")]
        [MaxLength(100)]
        public string Comment { get; set; }=string.Empty;

        [Required]
        public RatingReview RatingReview { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int TripId { get; set; }
        public Trip Trip { get; set; }

      
        public int GuideId { get; set; }
        public Guide Guide { get; set; }
    }
}
