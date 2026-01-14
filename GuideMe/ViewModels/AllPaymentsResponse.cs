using System.Collections.Generic;

namespace GuideMe.ViewModels
{
    public class AllPaymentsResponse
    {
        public IEnumerable<Payment> Payments { get; set; }
        public int CurrentPage { get; set; }
        public double PagesNumber { get; set; }
    }
}
