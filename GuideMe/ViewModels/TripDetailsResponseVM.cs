namespace GuideMe.ViewModels
{
    public class TripDetailsResponseVM
    {
        public Trip Trip { get; set; } = new Trip();
        public List<Offer> Offers { get; set; } = new List<Offer>();
        public List<Review> Reviews { get; set; } = new List<Review>();

        public ApplicationUser? CurrentUser { get; set; } 
    }
}
