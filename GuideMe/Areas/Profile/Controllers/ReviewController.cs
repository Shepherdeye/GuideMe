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

        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);
            
            if (visitor == null) return Forbid("Only visitors can manage reviews.");

            var reviews = await _reviewRepo.GetAsync(
                r => r.VisitorId == visitor.Id,
                includes: [r => r.Trip, r => r.Visitor, r => r.Visitor.ApplicationUser]);

            var sortedReviews = reviews.OrderByDescending(r => r.Id);
            var paginatedReviews = PaginatedList<GuideMe.Models.Review>.Create(sortedReviews, page, 8);

            return View(paginatedReviews);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);

            var review = await _reviewRepo.GetOneAsync(r => r.Id == id, includes: [r => r.Trip]);
            if (review == null) return NotFound();

            if (review.VisitorId != visitor?.Id) return Forbid();

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GuideMe.Models.Review review)
        {
            // Remove navigation properties from validation if they are causing issues
            ModelState.Remove("Visitor");
            ModelState.Remove("Trip");
            ModelState.Remove("Guide");
            ModelState.Remove("Booking");

            if (ModelState.IsValid)
            {
                var existingReview = await _reviewRepo.GetOneAsync(r => r.Id == review.Id);
                if (existingReview == null) return NotFound();

                // Check ownership again for security
                var userId = _userManager.GetUserId(User);
                var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);
                if (existingReview.VisitorId != visitor?.Id) return Forbid();

                // Update only editable fields to be safe
                existingReview.Comment = review.Comment;
                existingReview.RatingReview = review.RatingReview;
                existingReview.ReviewDate = DateTime.Now;

                _reviewRepo.Update(existingReview);
                await _reviewRepo.CommitAsync();
                
                TempData["Success"] = "Review updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            // If we got here, there's a validation error
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
