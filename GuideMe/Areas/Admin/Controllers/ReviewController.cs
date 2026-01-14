using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class ReviewController : Controller
    {
        private readonly IRepository<GuideMe.Models.Review> _reviewRepo;

        public ReviewController(IRepository<GuideMe.Models.Review> reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var reviews = await _reviewRepo.GetAsync(includes: [
                e => e.Visitor, 
                e => e.Visitor.ApplicationUser,
                e => e.Trip,
                e => e.Guide,
                e => e.Guide.ApplicationUser
            ]);

            var totalCount = reviews.Count();
            var pages = Math.Ceiling(totalCount / 10.00);
            reviews = reviews.OrderByDescending(e => e.Id).Skip((page - 1) * 10).Take(10).ToList();

            AllReviewsResponse data = new AllReviewsResponse
            {
                Reviews = reviews,
                CurrentPage = page,
                PagesNumber = pages
            };

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewRepo.GetOneAsync(e => e.Id == id);
            if (review == null) return NotFound();

            _reviewRepo.Delete(review);
            await _reviewRepo.CommitAsync();
            TempData["success-notification"] = "Review deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
