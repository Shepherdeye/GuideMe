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
    public class PaymentController : Controller
    {
        private readonly IRepository<Payment> _paymentRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PaymentController(
            IRepository<Payment> paymentRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _paymentRepo = paymentRepo;
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

            IEnumerable<Payment> payments;

            if (user.Role == UserRole.Guide && user.Guide != null)
            {
                // As a Guide, see payments where I am the guide in the associated booking
                payments = await _paymentRepo.GetAsync(
                    p => p.Booking.GuideId == user.Guide.Id,
                    includes: [p => p.Booking, p => p.Booking.Trip, p => p.Booking.Visitor, p => p.Booking.Visitor.ApplicationUser]);
                ViewBag.UserType = "Guide";
            }
            else if (user.Visitor != null)
            {
                // As a Visitor, see payments I made
                payments = await _paymentRepo.GetAsync(
                    p => p.Booking.VisitorId == user.Visitor.Id,
                    includes: [p => p.Booking, p => p.Booking.Trip, p => p.Booking.Guide, p => p.Booking.Guide.ApplicationUser]);
                ViewBag.UserType = "Visitor";
            }
            else
            {
                return Forbid();
            }

            return View(payments);
        }
    }
}
