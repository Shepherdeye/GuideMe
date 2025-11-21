namespace GuideMe.ViewModels
{
    public class CreateReviewVM
    {
        public int TripId { get; set; }
        public int VisitorId { get; set; }
        public RatingReview RatingReview { get; set; }
        public string? Comment { get; set; } = string.Empty;

    }
}
