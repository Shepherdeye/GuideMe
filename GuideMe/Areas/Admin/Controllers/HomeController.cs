using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.Utility;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    [Authorize(Roles = SD.AdminRole + "," + SD.SuperAdminRole)]
    public class HomeController : Controller
    {
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IRepository<Booking> bookingRepo,
            IRepository<Payment> paymentRepo,
            UserManager<ApplicationUser> userManager)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var payments = (await _paymentRepo.GetAsync())?.ToList() ?? new List<Payment>();
            var bookings = (await _bookingRepo.GetAsync())?.ToList() ?? new List<Booking>();
            var users = _userManager.Users?.ToList() ?? new List<ApplicationUser>();

            AdminDashboardVM model = new AdminDashboardVM
            {
                TotalRevenue = payments.Sum(p => p.PlatformEarning),
                TotalBookings = bookings.Count,
                TotalGuides = users.Count(u => u.Role == UserRole.Guide),
                TotalVisitors = users.Count(u => u.Role == UserRole.Visitor)
            };

            // Simple monthly aggregation (last 6 months)
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Now.AddMonths(-i);
                var monthLabel = monthDate.ToString("MMM yyyy");
                
                var monthBookings = bookings.Count(b => b.StartDate.Month == monthDate.Month && b.StartDate.Year == monthDate.Year);
                var monthRevenue = payments.Where(p => p.PaymentDate.Month == monthDate.Month && p.PaymentDate.Year == monthDate.Year)
                                          .Sum(p => p.PlatformEarning);

                model.MonthlyLabels.Add(monthLabel);
                model.MonthlyBookingCounts.Add(monthBookings);
                model.MonthlyRevenue.Add(monthRevenue);
            }

            return View(model);
        }

        public IActionResult NotFoundPage()
        {

            return View();
        }
    }
}
