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

        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.Visitor)
                .Include(u => u.Guide)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            IEnumerable<Payment> payments;

            if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.Admin)
            {
                // Ensure profiles exist
                bool changed = false;
                if (user.Guide == null) { user.Guide = new Guide { ApplicationUserId = userId }; await _context.Guides.AddAsync(user.Guide); changed = true; }
                if (user.Visitor == null) { user.Visitor = new Visitor { ApplicationUserId = userId, visitorStatus = VisitorStatus.Available }; await _context.Visitors.AddAsync(user.Visitor); changed = true; }
                if (changed) await _context.SaveChangesAsync();

                var guidePayments = await _paymentRepo.GetAsync(
                    p => p.Booking.GuideId == user.Guide.Id,
                    includes: [p => p.Booking, p => p.Booking.Trip, p => p.Booking.Visitor, p => p.Booking.Visitor.ApplicationUser]);
                
                var visitorPayments = await _paymentRepo.GetAsync(
                    p => p.Booking.VisitorId == user.Visitor.Id,
                    includes: [p => p.Booking, p => p.Booking.Trip, p => p.Booking.Guide, p => p.Booking.Guide.ApplicationUser]);

                payments = guidePayments.Concat(visitorPayments).DistinctBy(p => p.Id);
                ViewBag.UserRole = "SuperAdmin";
            }
            else if (user.Role == UserRole.Guide && user.Guide != null)
            {
                payments = await _paymentRepo.GetAsync(
                    p => p.Booking.GuideId == user.Guide.Id,
                    includes: [p => p.Booking, p => p.Booking.Trip, p => p.Booking.Visitor, p => p.Booking.Visitor.ApplicationUser]);
                ViewBag.UserRole = "Guide";
            }
            else if (user.Visitor != null)
            {
                payments = await _paymentRepo.GetAsync(
                    p => p.Booking.VisitorId == user.Visitor.Id,
                    includes: [p => p.Booking, p => p.Booking.Trip, p => p.Booking.Guide, p => p.Booking.Guide.ApplicationUser]);
                ViewBag.UserRole = "Visitor";
            }
            else
            {
                return Forbid();
            }

            // Financial Analytics
            var totalRevenue = payments.Sum(p => p.Amount);
            var totalFees = payments.Sum(p => p.PlatformEarning); // Total platform fee
            var netEarnings = payments.Sum(p => p.GuideEarning);
            
            if (user.Role == UserRole.Visitor) {
                totalFees = payments.Sum(p => p.ServiceFeeVisitor);
                netEarnings = totalRevenue - totalFees; // Cost for visitor, technically
            } else if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.Admin) {
                 // For SuperAdmin, show pure earnings where they are Guide, and costs where they are Visitor?
                 // Or just total volume. Let's show Net Earnings as (GuideEarnings - VisitorCosts)
                 // Or simpler: Show Guide Earning sum.
                 
                 // Split correctly:
                 var asGuide = payments.Where(p => p.Booking.GuideId == user.Guide?.Id);
                 var asVisitor = payments.Where(p => p.Booking.VisitorId == user.Visitor?.Id);
                 
                 var guideEarnings = asGuide.Sum(p => p.GuideEarning);
                 var visitorCosts = asVisitor.Sum(p => p.Amount); // Total spent
                 
                 netEarnings = guideEarnings; // Just show earnings for positivity
                 totalFees = asGuide.Sum(p => p.ServiceFeeGuide) + asVisitor.Sum(p => p.ServiceFeeVisitor);
            }

            var now = DateTime.Now;
            var thirtyDaysAgo = now.AddDays(-30);
            var sixtyDaysAgo = now.AddDays(-60);

            var currentPeriodEarnings = payments.Where(p => p.PaymentDate >= thirtyDaysAgo).Sum(p => p.Amount);
            var previousPeriodEarnings = payments.Where(p => p.PaymentDate >= sixtyDaysAgo && p.PaymentDate < thirtyDaysAgo).Sum(p => p.Amount);

            double growth = 0;
            if (previousPeriodEarnings > 0)
            {
                growth = (double)((currentPeriodEarnings - previousPeriodEarnings) / previousPeriodEarnings) * 100;
            }
            else if (currentPeriodEarnings > 0)
            {
                growth = 100;
            }

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalFees = totalFees;
            ViewBag.NetEarnings = netEarnings;
            ViewBag.Growth = growth;

            var sortedPayments = payments.OrderByDescending(p => p.Id);
            var paginatedPayments = PaginatedList<Payment>.Create(sortedPayments, page, 8);

            return View(paginatedPayments);
        }
    }
}
