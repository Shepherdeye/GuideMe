using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GuideMe.Utility;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    [Authorize(Roles = SD.AdminRole + "," + SD.SuperAdminRole)]
    public class BookingController : Controller
    {
        private readonly IRepository<Booking> _bookingRepo;

        public BookingController(IRepository<Booking> bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var bookings = await _bookingRepo.GetAsync(includes: [
                e => e.Trip, 
                e => e.Guide, 
                e => e.Guide.ApplicationUser, 
                e => e.Visitor, 
                e => e.Visitor.ApplicationUser
            ]);

            var totalCount = bookings.Count();
            var pages = Math.Ceiling(totalCount / 10.00);
            bookings = bookings.OrderByDescending(e => e.Id).Skip((page - 1) * 10).Take(10).ToList();

            AllBookingsResponse data = new AllBookingsResponse
            {
                Bookings = bookings,
                CurrentPage = page,
                PagesNumber = pages
            };

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _bookingRepo.GetOneAsync(e => e.Id == id, includes: [e => e.Trip, e => e.Guide, e => e.Visitor]);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Booking booking)
        {
            if (ModelState.IsValid)
            {
                _bookingRepo.Update(booking);
                await _bookingRepo.CommitAsync();
                TempData["success-notification"] = "Booking updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _bookingRepo.GetOneAsync(e => e.Id == id);
            if (booking == null) return NotFound();

            _bookingRepo.Delete(booking);
            await _bookingRepo.CommitAsync();
            TempData["success-notification"] = "Booking deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
