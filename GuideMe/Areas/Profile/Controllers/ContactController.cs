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
    public class ContactController : Controller
    {
        private readonly IRepository<ContactAccess> _contactAccessRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ContactController(
            IRepository<ContactAccess> contactAccessRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _contactAccessRepo = contactAccessRepo;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.Visitor)
                .Include(u => u.Guide)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            IEnumerable<ContactAccess> contacts;

            if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.Admin)
            {
                 // Ensure profiles exist (robustness)
                bool changed = false;
                if (user.Guide == null) { user.Guide = new Guide { ApplicationUserId = userId }; await _context.Guides.AddAsync(user.Guide); changed = true; }
                if (user.Visitor == null) { user.Visitor = new Visitor { ApplicationUserId = userId, visitorStatus = VisitorStatus.Available }; await _context.Visitors.AddAsync(user.Visitor); changed = true; }
                if (changed) await _context.SaveChangesAsync();

                var guideContacts = await _contactAccessRepo.GetAsync(
                    c => c.Booking.GuideId == user.Guide.Id,
                     includes: [c => c.Booking, c => c.Booking.Visitor, c => c.Booking.Visitor.ApplicationUser, c => c.Booking.Trip]);

                var visitorContacts = await _contactAccessRepo.GetAsync(
                    c => c.Booking.VisitorId == user.Visitor.Id,
                    includes: [c => c.Booking, c => c.Booking.Guide, c => c.Booking.Guide.ApplicationUser, c => c.Booking.Trip]);

                contacts = guideContacts.Concat(visitorContacts).DistinctBy(c => c.Id);
                ViewBag.CurrentUserRole = "SuperAdmin";
            }
            else if (user.Role == UserRole.Guide && user.Guide != null)
            {
                contacts = await _contactAccessRepo.GetAsync(
                    c => c.Booking.GuideId == user.Guide.Id,
                    includes: [c => c.Booking, c => c.Booking.Visitor, c => c.Booking.Visitor.ApplicationUser, c => c.Booking.Trip]);
                ViewBag.CurrentUserRole = "Guide";
            }
            else if (user.Visitor != null)
            {
                contacts = await _contactAccessRepo.GetAsync(
                    c => c.Booking.VisitorId == user.Visitor.Id,
                    includes: [c => c.Booking, c => c.Booking.Guide, c => c.Booking.Guide.ApplicationUser, c => c.Booking.Trip]);
                ViewBag.CurrentUserRole = "Visitor";
            }
            else
            {
                return Forbid();
            }

            var sortedContacts = contacts.OrderByDescending(c => c.Id);
            var paginatedContacts = PaginatedList<ContactAccess>.Create(sortedContacts, page, 8);

            return View(paginatedContacts);
        }
    }
}
