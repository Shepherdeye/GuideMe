using GuideMe.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Areas.Profile.Controllers
{
    [Area(SD.ProfileArea)]
    public class TripController : Controller
    {
        private readonly IRepository<Trip> _tripRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TripController(
            IRepository<Trip> tripRepository,
            UserManager<ApplicationUser> userManager
            , ApplicationDbContext context)
        {
            _tripRepository = tripRepository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {

            var userId = _userManager.GetUserId(User);

            if (userId == null) return NotFound();


            var currentUser = await _context.Users.Include(e => e.Visitor).Include(e => e.Guide)
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (currentUser == null) return NotFound();

            var trips = await _tripRepository.GetAsync(e => e.VisitorId == currentUser.Visitor.Id, includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);

            var totalCount = trips.Count();
            double pagesNumber = Math.Ceiling(totalCount / 4.00);

            trips = trips.Skip((page - 1) * 4).Take(4).ToList();

            AllTripData data = new AllTripData
            {
                Trips = trips,
                CurrentPage = page,
                PagesNumber = pagesNumber
            };



            return View(data);
        }
    }
}
