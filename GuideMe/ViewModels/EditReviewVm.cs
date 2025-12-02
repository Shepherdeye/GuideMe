using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class EditReviewVm
    {
        public int ReviewId { get; set; }
        [Required]
        public string Comment { get; set; } = string.Empty;

        [Required]
        public RatingReview RatingReview { get; set; }

        public int TripId { get; set; }

    }
}
