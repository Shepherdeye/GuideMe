namespace GuideMe.ViewModels
{
    public class AllVisitorResponse
    {
        public List<VisitorResponseVM> Visitors { get; set; }
        public int TotalCount { get; set; }

        public double PagesNumber { get; set; }

        public int CurrentPage { get; set; }
    }
}
