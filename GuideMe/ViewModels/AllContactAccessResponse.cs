using System.Collections.Generic;

namespace GuideMe.ViewModels
{
    public class AllContactAccessResponse
    {
        public IEnumerable<ContactAccess> ContactAccesses { get; set; }
        public int CurrentPage { get; set; }
        public double PagesNumber { get; set; }
    }
}
