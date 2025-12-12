using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class CreateReviewVM
    {
        public int TripId { get; set; }
        public int VisitorId { get; set; }
        [Required]
        public RatingReview RatingReview { get; set; }
        [Required]
        public string? Comment { get; set; } = string.Empty;

    }
}
