using GuideMe.Repositories;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class PaymentController : Controller
    {
        private readonly IRepository<Payment> _paymentRepo;

        public PaymentController(IRepository<Payment> paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var payments = await _paymentRepo.GetAsync(includes: [
                e => e.Booking,
                e => e.Booking.Trip,
                e => e.Booking.Visitor,
                e => e.Booking.Visitor.ApplicationUser
            ]);

            var totalCount = payments.Count();
            var pages = Math.Ceiling(totalCount / 10.00);
            payments = payments.OrderByDescending(e => e.Id).Skip((page - 1) * 10).Take(10).ToList();

            AllPaymentsResponse data = new AllPaymentsResponse
            {
                Payments = payments,
                CurrentPage = page,
                PagesNumber = pages
            };

            return View(data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentRepo.GetOneAsync(e => e.Id == id, includes: [
                e => e.Booking,
                e => e.Booking.Trip,
                e => e.Booking.Visitor,
                e => e.Booking.Visitor.ApplicationUser,
                e => e.Booking.Guide,
                e => e.Booking.Guide.ApplicationUser
            ]);
            if (payment == null) return NotFound();
            return View(payment);
        }
    }
}
