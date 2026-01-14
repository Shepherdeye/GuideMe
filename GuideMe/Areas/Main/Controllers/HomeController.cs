using GuideMe.DataAccess;
using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.Utility;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Areas.Main.Controllers
{
    [Area(SD.MainArea)]
    public class HomeController : Controller
    {
        private readonly IRepository<Trip> _tripRepo;
        private readonly IRepository<Guide> _guideRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly ApplicationDbContext _context;

        public HomeController(
            IRepository<Trip> tripRepo,
            IRepository<Guide> guideRepo,
            IRepository<Review> reviewRepo,
            ApplicationDbContext context)
        {
            _tripRepo = tripRepo;
            _guideRepo = guideRepo;
            _reviewRepo = reviewRepo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var popularTrips = await _tripRepo.GetAsync(
                t => t.Status == TripStatus.Open,
                includes: [t => t.Visitor, t => t.Visitor.ApplicationUser]);
            
            var featuredGuides = await _guideRepo.GetAsync(
                includes: [g => g.ApplicationUser]);

            var recentReviews = await _reviewRepo.GetAsync(
                includes: [r => r.Visitor, r => r.Visitor.ApplicationUser, r => r.Trip]);

            var model = new HomeVM
            {
                PopularTrips = popularTrips.OrderByDescending(t => t.Id).Take(3),
                FeaturedGuides = featuredGuides.Take(4),
                RecentReviews = recentReviews.OrderByDescending(r => r.ReviewDate).Take(4),
                TotalTrips = _context.Trips.Count(),
                TotalGuides = featuredGuides.Count(),
                SatisfiedVisitors = _context.Visitors.Count()
            };

            return View(model);
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
