using GuideMe.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Main.Controllers
{
    [Area(SD.MainArea)]
    public class TripController : Controller
    {
        private readonly IRepository<Trip> _tripRepo;
        private readonly IRepository<Offer> _offerRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public TripController(
              IRepository<Trip> tripRepo
            , IRepository<Offer> offerRepo
            , IRepository<Review> ReviewRepo
            , UserManager<ApplicationUser> userManager
            )
        {
            _tripRepo = tripRepo;
            _offerRepo = offerRepo;
            _reviewRepo = ReviewRepo;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(TripFilterVM filter, int page = 1)
        {
            var trips = await _tripRepo.GetAsync();

            if (!string.IsNullOrEmpty(filter.Title))
            {

                trips = trips.Where(e => e.Title.Contains(filter.Title)).ToList();
            }

            if (!string.IsNullOrEmpty(filter.Distination))
            {
                trips = trips.Where(e => e.Destination.Contains(filter.Distination)).ToList();
            }

            if (filter.MinPrice is not null && filter.MinPrice != 0)
            {
                trips = trips.Where(e => e.Price >= filter.MinPrice).ToList();
            }

            if (filter.MaxPrice is not null && filter.MaxPrice != 0)
            {
                trips = trips.Where(e => e.Price <= filter.MaxPrice).ToList();
            }

            if (filter.Active == true)
            {
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

        public async Task<IActionResult> Details(int id)
        {
            var trip = await _tripRepo.GetOneAsync(e => e.Id == id, includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);
            if (trip == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Main" });
            var offers = await _offerRepo.GetAsync(e => e.TripId == trip.Id, includes: [e => e.Guide, e => e.Guide.ApplicationUser]);
            var reviews = await _reviewRepo.GetAsync(e => e.TripId == trip.Id, includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            TripDetailsResponseVM data = new TripDetailsResponseVM()
            {
                Trip = trip,
                Offers = offers,
                Reviews = reviews,
                CurrentUser = user

            };


            return View(data);
        }
    }
}
