using System.Collections.Generic;

namespace GuideMe.ViewModels
{
    public class AllBookingsResponse
    {
        public IEnumerable<Booking> Bookings { get; set; }
        public int CurrentPage { get; set; }
        public double PagesNumber { get; set; }
    }
}
