namespace GuideMe.ViewModels
{
    public class AllGuideResponse
    {
        public List<GuideResponseVM> Guides { get; set; }
        public int TotalCount { get; set; }

        public double PagesNumber { get; set; }

        public int CurrentPage { get; set; }
    }
}
