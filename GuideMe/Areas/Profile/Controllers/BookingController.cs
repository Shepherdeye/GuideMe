using GuideMe.DataAccess;
using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace GuideMe.Areas.Profile.Controllers
{
    [Area(SD.ProfileArea)]
    public class BookingController : Controller
    {
        private readonly IRepository<Booking> _bookingRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BookingController(
            IRepository<Booking> bookingRepo, 
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _bookingRepo = bookingRepo;
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

            IEnumerable<Booking> bookings;

            if (user.Role == UserRole.Guide && user.Guide != null)
            {
                bookings = await _bookingRepo.GetAsync(
                    b => b.GuideId == user.Guide.Id,
                    includes: [b => b.Trip, b => b.Visitor, b => b.Visitor.ApplicationUser]);
                ViewBag.UserType = "Guide";
            }
            else if (user.Visitor != null)
            {
                bookings = await _bookingRepo.GetAsync(
                    b => b.VisitorId == user.Visitor.Id,
                    includes: [b => b.Trip, b => b.Guide, b => b.Guide.ApplicationUser]);
                ViewBag.UserType = "Visitor";
            }
            else
            {
                return Forbid();
            }

            var sortedBookings = bookings.OrderByDescending(b => b.Id);
            var paginatedBookings = PaginatedList<Booking>.Create(sortedBookings, page, 6);

            return View(paginatedBookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.Visitor)
                .Include(u => u.Guide)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var booking = await _bookingRepo.GetOneAsync(
                b => b.Id == id,
                includes: [b => b.Trip, b => b.Guide, b => b.Guide.ApplicationUser, b => b.Visitor, b => b.Visitor.ApplicationUser]);

            if (booking == null) return NotFound();

            // Security check
            if (booking.VisitorId != user?.Visitor?.Id && booking.GuideId != user?.Guide?.Id)
            {
                return Forbid();
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ApplicationUserId == userId);
            
            var booking = await _bookingRepo.GetOneAsync(b => b.Id == id);
            if (booking == null) return NotFound();

            if (booking.VisitorId != visitor?.Id) return Forbid();

            if (booking.BookingStatus == BookingStatus.Pending)
            {
                booking.BookingStatus = BookingStatus.Canceled;
                _bookingRepo.Update(booking);
                await _bookingRepo.CommitAsync();
                TempData["Success"] = "Booking canceled successfully.";
            }
            else
            {
                TempData["Error"] = "Only pending bookings can be canceled.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, BookingStatus status)
        {
            var userId = _userManager.GetUserId(User);
            var guide = await _context.Guides.FirstOrDefaultAsync(g => g.ApplicationUserId == userId);

            var booking = await _bookingRepo.GetOneAsync(b => b.Id == id);
            if (booking == null) return NotFound();

            if (booking.GuideId != guide?.Id) return Forbid();

            booking.BookingStatus = status;
            _bookingRepo.Update(booking);
            await _bookingRepo.CommitAsync();
            TempData["Success"] = $"Booking {status} successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var booking = await _bookingRepo.GetOneAsync(b => b.Id == id, includes: [b => b.Trip]);
            if (booking == null) return NotFound();

            if (booking.BookingStatus != BookingStatus.Accepted)
            {
                TempData["Error"] = "Only accepted bookings can be paid for.";
                return RedirectToAction(nameof(Index));
            }

            var domain = $"{Request.Scheme}://{Request.Host.Value}/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Profile/Booking/PaymentConfirmation?id={id}",
                CancelUrl = domain + $"Profile/Booking/Index",
            };

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.BookingPrice * 100), // Stripe uses cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = booking.Trip.Title,
                        Description = $"Booking from {booking.StartDate.ToShortDateString()} to {booking.EndDate.ToShortDateString()}"
                    },
                },
                Quantity = 1,
            };
            options.LineItems.Add(sessionLineItem);

            var service = new SessionService();
            Session session = service.Create(options);

            booking.StripeSessionId = session.Id;
            _bookingRepo.Update(booking);
            await _bookingRepo.CommitAsync();

            if (session.Url != null)
            {
                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return BadRequest("Could not create payment session.");
        }

        public async Task<IActionResult> PaymentConfirmation(int id)
        {
            var booking = await _bookingRepo.GetOneAsync(b => b.Id == id, includes: [b => b.Trip, b => b.Visitor, b => b.Guide]);
            if (booking == null) return NotFound();

            if (string.IsNullOrEmpty(booking.StripeSessionId)) return BadRequest("Invalid session.");

            var service = new SessionService();
            Session session = service.Get(booking.StripeSessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                bool isNewPayment = false;
                
                // Idempotency check: Create Payment Record only if it doesn't exist
                var paymentRepo = HttpContext.RequestServices.GetRequiredService<IRepository<Payment>>();
                var existingPayment = await paymentRepo.GetOneAsync(p => p.BookingId == id);
                
                if (existingPayment == null)
                {
                    decimal totalAmount = booking.BookingPrice;
                    decimal platformEarning = totalAmount * 0.10m;
                    decimal guideEarning = totalAmount - platformEarning;

                    Payment payment = new Payment
                    {
                        BookingId = booking.Id,
                        Amount = totalAmount,
                        StripePaymentIntentId = session.PaymentIntentId,
                        PaymentDate = DateTime.Now,
                        PlatformEarning = platformEarning,
                        GuideEarning = guideEarning,
                        ServiceFeeGuide = platformEarning / 2,
                        ServiceFeeVisitor = platformEarning / 2
                    };

                    await paymentRepo.CreateAsync(payment);
                    isNewPayment = true;
                }

                // Idempotency check: Create ContactAccess Record only if it doesn't exist (prevents IX_ContactAccess_BookingId error)
                var contactRepo = HttpContext.RequestServices.GetRequiredService<IRepository<ContactAccess>>();
                var existingContact = await contactRepo.GetOneAsync(c => c.BookingId == id);
                
                if (existingContact == null)
                {
                    ContactAccess contactAccess = new ContactAccess
                    {
                        BookingId = booking.Id,
                        CreatedAt = DateTime.Now
                    };
                    await contactRepo.CreateAsync(contactAccess);
                }

                // Update booking status if not already set
                if (booking.BookingStatus != BookingStatus.Paid)
                {
                    booking.BookingStatus = BookingStatus.Paid;
                    _bookingRepo.Update(booking);
                }

                await _bookingRepo.CommitAsync();

                if (isNewPayment)
                {
                    TempData["Success"] = "Payment successful! Your trip is confirmed and contact information is unlocked.";
                }
            }

            return View(id);
        }
    }
}
