using GuideMe.DataAccess;
using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Areas.Profile.Controllers
{
    [Area(SD.ProfileArea)]
    public class OfferController : Controller
    {
        private readonly IRepository<Offer> _offerRepo;
        private readonly IRepository<Trip> _tripRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OfferController(
            IRepository<Offer> offerRepo,
            IRepository<Trip> tripRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _offerRepo = offerRepo;
            _tripRepo = tripRepo;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.Visitor)
                .Include(u => u.Guide)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            IEnumerable<Offer> offers;

            if (user.Role == UserRole.Guide && user.Guide != null)
            {
                // As a Guide, see offers I have SENT
                offers = await _offerRepo.GetAsync(
                    o => o.GuideId == user.Guide.Id,
                    includes: [o => o.Trip, o => o.Trip.Visitor, o => o.Trip.Visitor.ApplicationUser]);
                ViewBag.UserType = "Guide";
            }
            else if (user.Visitor != null)
            {
                // As a Visitor, see offers RECEIVED on my trips
                offers = await _offerRepo.GetAsync(
                    o => o.Trip.VisitorId == user.Visitor.Id,
                    includes: [o => o.Trip, o => o.Guide, o => o.Guide.ApplicationUser]);
                ViewBag.UserType = "Visitor";
            }
            else
            {
                return Forbid();
            }

            return View(offers);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? tripId)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.Include(u => u.Guide).FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user?.Role != UserRole.Guide) return Forbid("Only guides can create offers.");

            if (tripId.HasValue)
            {
                var trip = await _tripRepo.GetOneAsync(t => t.Id == tripId.Value);
                if (trip == null) return NotFound();
                ViewBag.TripTitle = trip.Title;
                return View(new Offer { TripId = tripId.Value, GuideId = user.Guide.Id, Status = OfferStatus.Pending });
            }

            var trips = await _tripRepo.GetAsync(t => t.Status == TripStatus.Open);
            ViewBag.Trips = new SelectList(trips, "Id", "Title");
            return View(new Offer { GuideId = user.Guide.Id, Status = OfferStatus.Pending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offer offer)
        {
            if (ModelState.IsValid)
            {
                offer.Status = OfferStatus.Pending;
                await _offerRepo.CreateAsync(offer);
                await _offerRepo.CommitAsync();
                TempData["Success"] = "Offer sent successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(offer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OfferStatus status)
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);

            var offer = await _offerRepo.GetOneAsync(o => o.Id == id, includes: [o => o.Trip]);
            if (offer == null) return NotFound();

            // Only Trip Owner (Visitor) can Accept/Reject
            if (offer.Trip.VisitorId != visitor?.Id) return Forbid();

            offer.Status = status;
            _offerRepo.Update(offer);
            await _offerRepo.CommitAsync();
            TempData["Success"] = $"Offer {status} successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var guide = await _context.Guides.FirstOrDefaultAsync(g => g.ApplicationUserId == userId);

            var offer = await _offerRepo.GetOneAsync(o => o.Id == id);
            if (offer == null) return NotFound();

            if (offer.GuideId != guide?.Id) return Forbid();

            _offerRepo.Delete(offer);
            await _offerRepo.CommitAsync();
            TempData["Success"] = "Offer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
