using GuideMe.DataAccess;
using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Areas.Profile.Controllers
{
    [Area(SD.ProfileArea)]
    public class ReviewController : Controller
    {
        private readonly IRepository<GuideMe.Models.Review> _reviewRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ReviewController(
            IRepository<GuideMe.Models.Review> reviewRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _reviewRepo = reviewRepo;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);
            
            if (visitor == null) return Forbid("Only visitors can write reviews.");

            var reviews = await _reviewRepo.GetAsync(
                r => r.VisitorId == visitor.Id,
                includes: [r => r.Trip, r => r.Guide, r => r.Guide.ApplicationUser]);

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);

            var review = await _reviewRepo.GetOneAsync(r => r.Id == id);
            if (review == null) return NotFound();

            if (review.VisitorId != visitor?.Id) return Forbid();

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GuideMe.Models.Review review)
        {
            if (ModelState.IsValid)
            {
                var existingReview = await _reviewRepo.GetOneAsync(r => r.Id == review.Id, tracked: false);
                if (existingReview == null) return NotFound();

                review.VisitorId = existingReview.VisitorId;
                review.TripId = existingReview.TripId;
                review.GuideId = existingReview.GuideId;
                review.BookingId = existingReview.BookingId;
                review.ReviewDate = DateTime.Now;

                _reviewRepo.Update(review);
                await _reviewRepo.CommitAsync();
                TempData["Success"] = "Review updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);

            var review = await _reviewRepo.GetOneAsync(r => r.Id == id);
            if (review == null) return NotFound();

            if (review.VisitorId != visitor?.Id) return Forbid();

            _reviewRepo.Delete(review);
            await _reviewRepo.CommitAsync();
            TempData["Success"] = "Review deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
