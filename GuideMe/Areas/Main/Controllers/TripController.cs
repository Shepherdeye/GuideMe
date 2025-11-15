using GuideMe.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;

namespace GuideMe.Areas.Main.Controllers
{
    [Area(SD.MainArea)]
    public class TripController : Controller
    {
        private readonly IRepository<Trip> _tripRepo;

        public TripController(IRepository<Trip> tripRepo)
        {
            _tripRepo = tripRepo;
        }
        public async Task<IActionResult> Index(TripFilterVM filter, int page = 1)
        {
            var trips = await _tripRepo.GetAsync(includes: [e => e.Visitor]);

            if (!string.IsNullOrEmpty(filter.Title))
            {
                //var title = filter.Title;
                trips = trips.Where(e => e.Title.Contains(filter.Title)).ToList();
            }
            if (!string.IsNullOrEmpty(filter.Distination))
            {
                //var distination = filter.Distination;
                trips = trips.Where(e => e.Destination.Contains(filter.Distination)).ToList();
            }
            if (filter.MinPrice is not null && filter.MinPrice != 0)
            {
                //var price = filter.MinPrice;
                trips = trips.Where(e => e.Price >= filter.MinPrice).ToList();
            }
            if (filter.MaxPrice is not null && filter.MaxPrice != 0)
            {
                //var price = filter.MaxPrice;
                trips = trips.Where(e => e.Price <= filter.MaxPrice).ToList();
            }
            if (filter.Active == true)
            {
                //var title = filter.Title;
                trips = trips.Where(e => e.Status == TripStatus.Open).ToList();
            }

            var totalcount = trips.Count();
            var totalpages = Math.Ceiling(totalcount / 6.00);
            trips = trips.Skip((page - 1) * 6).Take(6).ToList();

            TripsWithFilterVm Data = new TripsWithFilterVm()
            {
                PagesNumber = totalpages,
                CurrentPage = page,
                Trips = trips,
                Filter = filter

            };

            return View(Data);
        }

        public  IActionResult Details()
        {
            return View();
        }
    }
}
