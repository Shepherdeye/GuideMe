using System.Collections.Generic;

namespace GuideMe.ViewModels
{
    public class AllReviewsResponse
    {
        public IEnumerable<Review> Reviews { get; set; }
        public int CurrentPage { get; set; }
        public double PagesNumber { get; set; }
    }
}
