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
            var currentUser = await _context.Users.Include(u => u.Visitor).FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null) return NotFound();

            IEnumerable<GuideMe.Models.Review> reviews;

            // Handle SuperAdmin/Admin
            if (currentUser.Role == UserRole.SuperAdmin || currentUser.Role == UserRole.Admin)
            {
                if (currentUser.Guide == null)
                {
                   currentUser.Guide = new Guide { ApplicationUserId = userId };
                   await _context.Guides.AddAsync(currentUser.Guide);
                }
                if (currentUser.Visitor == null)
                {
                    currentUser.Visitor = new Visitor { ApplicationUserId = userId, visitorStatus = VisitorStatus.Available };
                    await _context.Visitors.AddAsync(currentUser.Visitor);
                }
                await _context.SaveChangesAsync();

                var writtenReviews = await _reviewRepo.GetAsync(
                    r => r.VisitorId == currentUser.Visitor.Id,
                    includes: [r => r.Trip, r => r.Visitor, r => r.Visitor.ApplicationUser, r => r.Guide, r => r.Guide.ApplicationUser]);
                
                var receivedReviews = await _reviewRepo.GetAsync(
                    r => r.GuideId == currentUser.Guide.Id,
                    includes: [r => r.Trip, r => r.Visitor, r => r.Visitor.ApplicationUser, r => r.Guide, r => r.Guide.ApplicationUser]);

                reviews = writtenReviews.Concat(receivedReviews).DistinctBy(r => r.Id);
                ViewBag.UserType = "SuperAdmin";
            }
            else if (currentUser.Role == UserRole.Guide && currentUser.Guide != null)
            {
                 // Guide might also have a Visitor profile if they book trips, so check both if possible, 
                 // but typically Guide primarily cares about received reviews. 
                 // However, "like both" implies if I am a Guide and I acted as a Visitor, I want both.
                 // Let's safe-guard:
                 
                 var receivedReviews = await _reviewRepo.GetAsync(
                    r => r.GuideId == currentUser.Guide.Id,
                     includes: [r => r.Trip, r => r.Visitor, r => r.Visitor.ApplicationUser, r => r.Guide, r => r.Guide.ApplicationUser]);
                 
                 var writtenReviews = Enumerable.Empty<GuideMe.Models.Review>();
                 if (currentUser.Visitor != null)
                 {
                     writtenReviews = await _reviewRepo.GetAsync(
                        r => r.VisitorId == currentUser.Visitor.Id,
                        includes: [r => r.Trip, r => r.Visitor, r => r.Visitor.ApplicationUser, r => r.Guide, r => r.Guide.ApplicationUser]);
                 }

                 reviews = receivedReviews.Concat(writtenReviews).DistinctBy(r => r.Id);
                 ViewBag.UserType = "Guide";
            }
            else if (currentUser.Visitor != null)
            {
                 reviews = await _reviewRepo.GetAsync(
                    r => r.VisitorId == currentUser.Visitor.Id,
                    includes: [r => r.Trip, r => r.Visitor, r => r.Visitor.ApplicationUser, r => r.Guide, r => r.Guide.ApplicationUser]);
                 ViewBag.UserType = "Visitor";
            }
            else
            {
                return Forbid();
            }

            var sortedReviews = reviews.OrderByDescending(r => r.Id);
            var paginatedReviews = PaginatedList<GuideMe.Models.Review>.Create(sortedReviews, page, 8);

            return View(paginatedReviews);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var currentUser = await _context.Users.Include(u => u.Visitor).FirstOrDefaultAsync(u => u.Id == userId);
            
            var visitorId = currentUser?.Visitor?.Id;

            var review = await _reviewRepo.GetOneAsync(r => r.Id == id, includes: [r => r.Trip]);
            if (review == null) return NotFound();

            if (review.VisitorId != visitorId) return Forbid();

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
                var currentUser = await _context.Users.Include(u => u.Visitor).FirstOrDefaultAsync(u => u.Id == userId);
                if (existingReview.VisitorId != currentUser?.Visitor?.Id) return Forbid();

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
            var currentUser = await _context.Users.Include(u => u.Visitor).FirstOrDefaultAsync(u => u.Id == userId);

            var review = await _reviewRepo.GetOneAsync(r => r.Id == id);
            if (review == null) return NotFound();

            if (review.VisitorId != currentUser?.Visitor?.Id) return Forbid();

            _reviewRepo.Delete(review);
            await _reviewRepo.CommitAsync();
            TempData["Success"] = "Review deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}

