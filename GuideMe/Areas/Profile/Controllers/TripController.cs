using GuideMe.DataAccess;
using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.Utility;
using GuideMe.ViewModels;
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
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
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

            // Handle SuperAdmin/Admin who might not have a Visitor record yet
            if (currentUser.Visitor == null && (currentUser.Role == UserRole.SuperAdmin || currentUser.Role == UserRole.Admin))
            {
                var visitor = new Visitor { ApplicationUserId = userId, visitorStatus = VisitorStatus.Available };
                await _context.Visitors.AddAsync(visitor);
                await _context.SaveChangesAsync();
                currentUser.Visitor = visitor;
            }

            if (currentUser.Visitor == null) return NotFound("Visitor profile not found.");

            if (currentUser.Visitor == null) return NotFound("Visitor profile not found.");

            IEnumerable<Trip> trips;

            if (currentUser.Role == UserRole.SuperAdmin || currentUser.Role == UserRole.Admin)
            {
               // Get trips created by this user (as Visitor)
               var createdTrips = await _tripRepository.GetAsync(
                   e => e.VisitorId == currentUser.Visitor.Id, 
                   includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);
               
               // Get trips where this user is the confirmed Guide (via Bookings)
               // Note: We need to load Bookings to filter effectively if repository doesn't support complex cross-linking easily.
               // However, simple way is: if we have a Booking repository, we query that. 
               // Since we are in TripController and usually only inject TripRepo, let's see if we can query Trips with a predicate involving Bookings.
               // Default Generic Repo might not support deep nested filtering if not set up, but let's try.
               // Alternatively, filtering in memory if dataset is small, or injecting BookingRepo.
               // Let's assume we want to show trips where the guide has an Accepted booking.
               
               // For now, to avoid major dependency injection changes if not present, let's keep it simple.
               // But wait, TripController only has TripRepo.
               // Actually, `GetAsync` takes a predicate.
               // `e => e.Bookings.Any(b => b.GuideId == currentUser.Guide.Id)` might work if EF Core is handling it.
               
               var guidedTrips = await _tripRepository.GetAsync(
                   e => e.Bookings.Any(b => b.GuideId == currentUser.Guide.Id && (b.BookingStatus == BookingStatus.Accepted || b.BookingStatus == BookingStatus.Paid)),
                   includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]
               );

               trips = createdTrips.Concat(guidedTrips).DistinctBy(t => t.Id);
               ViewBag.UserType = "SuperAdmin";
            }
            else if (currentUser.Role == UserRole.Guide)
            {
                 // Guide might have created trips as a Visitor too?
                 var guidedTrips = await _tripRepository.GetAsync(
                   e => e.Bookings.Any(b => b.GuideId == currentUser.Guide.Id && (b.BookingStatus == BookingStatus.Accepted || b.BookingStatus == BookingStatus.Paid)),
                   includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]
                 );
                 
                 var createdTrips = Enumerable.Empty<Trip>();
                 if (currentUser.Visitor != null)
                 {
                    createdTrips = await _tripRepository.GetAsync(
                       e => e.VisitorId == currentUser.Visitor.Id, 
                       includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);
                 }
                 
                 trips = guidedTrips.Concat(createdTrips).DistinctBy(t => t.Id);
                 ViewBag.UserType = "Guide";
            }
            else
            {
                // Regular Visitor
                trips = await _tripRepository.GetAsync(
                    e => e.VisitorId == currentUser.Visitor.Id, 
                    includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);
                ViewBag.UserType = "Visitor";
            }

            var sortedTrips = trips.OrderByDescending(t => t.Id);
            var paginatedTrips = PaginatedList<Trip>.Create(sortedTrips, page, 6);

            return View(paginatedTrips);
        }

        public async Task<IActionResult> Details(int id)
        {
            var trip = await _tripRepository.GetOneAsync(t => t.Id == id, includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);
            if (trip == null) return NotFound();
            return View(trip);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trip trip, IFormFile? Image)
        {
            var userId = _userManager.GetUserId(User);
            var currentUser = await _context.Users.Include(e => e.Visitor).FirstOrDefaultAsync(e => e.Id == userId);
            
            if (currentUser == null) return NotFound();

            // Handle SuperAdmin/Admin
            if (currentUser.Visitor == null && (currentUser.Role == UserRole.SuperAdmin || currentUser.Role == UserRole.Admin))
            {
                var visitor = new Visitor { ApplicationUserId = userId, visitorStatus = VisitorStatus.Available };
                await _context.Visitors.AddAsync(visitor);
                await _context.SaveChangesAsync();
                currentUser.Visitor = visitor;
            }

            if (currentUser.Visitor == null) return NotFound("Visitor profile not found.");

            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    var filename = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\main\\img", filename);

                    using (var stream = System.IO.File.Create(filepath))
                    {
                        await Image.CopyToAsync(stream);
                    }
                    trip.Image = filename;
                }

                trip.VisitorId = currentUser.Visitor.Id;
                trip.CreatedOn = DateTime.Now;
                trip.LastUpdatedon = DateTime.Now;

                await _tripRepository.CreateAsync(trip);
                await _tripRepository.CommitAsync();
                TempData["Success"] = "Trip created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var trip = await _tripRepository.GetOneAsync(t => t.Id == id);
            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Trip trip, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                var existingTrip = await _tripRepository.GetOneAsync(t => t.Id == trip.Id, tracked: false);
                if (existingTrip == null) return NotFound();

                if (Image != null && Image.Length > 0)
                {
                    var filename = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\main\\img", filename);

                    using (var stream = System.IO.File.Create(filepath))
                    {
                        await Image.CopyToAsync(stream);
                    }
                    trip.Image = filename;
                }
                else
                {
                    trip.Image = existingTrip.Image;
                }

                trip.VisitorId = existingTrip.VisitorId;
                trip.CreatedOn = existingTrip.CreatedOn;
                trip.LastUpdatedon = DateTime.Now;

                _tripRepository.Update(trip);
                await _tripRepository.CommitAsync();
                TempData["Success"] = "Trip updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _tripRepository.GetOneAsync(t => t.Id == id);
            if (trip == null) return NotFound();

            _tripRepository.Delete(trip);
            await _tripRepository.CommitAsync();
            TempData["Success"] = "Trip deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
