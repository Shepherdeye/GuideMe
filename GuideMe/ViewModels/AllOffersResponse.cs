namespace GuideMe.ViewModels
{
    public class AllOffersResponse
    {
        public List<Offer> Offers { get; set; } = new List<Offer>();

        public double PagesNumber { get; set; } = 0.00;

        public int CurrentPage { get; set; } = 1;
    }
}
