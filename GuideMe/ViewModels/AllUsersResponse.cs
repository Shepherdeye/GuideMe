using System.Collections.Generic;

namespace GuideMe.ViewModels
{
    public class AllUsersResponse
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public int CurrentPage { get; set; }
        public double PagesNumber { get; set; }
    }
}
