using GuideMe.Models;

namespace GuideMe.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Trip> PopularTrips { get; set; } = new List<Trip>();
        public IEnumerable<Guide> FeaturedGuides { get; set; } = new List<Guide>();
        public IEnumerable<Review> RecentReviews { get; set; } = new List<Review>();
        
        // Stats for the "About" or "Counter" section if needed
        public int TotalTrips { get; set; }
        public int TotalGuides { get; set; }
        public int SatisfiedVisitors { get; set; }
    }
}
