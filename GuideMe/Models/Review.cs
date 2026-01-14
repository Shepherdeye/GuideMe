using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required(ErrorMessage = "Comment is required.")]
        [MinLength(10, ErrorMessage = "Comment must be at least 10 characters.")]
        [MaxLength(200, ErrorMessage = "Comment cannot exceed 200 characters.")]
        public string Comment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rating is required.")]
        public RatingReview RatingReview { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Visitor is required.")]
        public int VisitorId { get; set; }

        [ForeignKey(nameof(VisitorId))]
        public Visitor? Visitor { get; set; }


        public int? BookingId { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }


        public int? TripId { get; set; }

        [ForeignKey(nameof(TripId))]
        public Trip? Trip { get; set; }


        public int? GuideId { get; set; }

        [ForeignKey(nameof(GuideId))]
        public Guide? Guide { get; set; }
    }

}
