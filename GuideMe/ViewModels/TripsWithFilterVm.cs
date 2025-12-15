namespace GuideMe.ViewModels
{
    public class TripsWithFilterVm
    {
        public List<Trip> Trips { get; set; } = new List<Trip>();
        public double PagesNumber { get; set; }
        public int CurrentPage { get; set; }
        public TripFilterVM? Filter { get; set; }
        public ApplicationUser? CurrentUser { get; set; }

    }
}
