using GuideMe.Repositories;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class ContactController : Controller
    {
        private readonly IRepository<ContactAccess> _contactAccessRepo;

        public ContactController(IRepository<ContactAccess> contactAccessRepo)
        {
            _contactAccessRepo = contactAccessRepo;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var contacts = await _contactAccessRepo.GetAsync(includes: [
                e => e.Booking,
                e => e.Booking.Trip,
                e => e.Booking.Visitor,
                e => e.Booking.Visitor.ApplicationUser,
                e => e.Booking.Guide,
                e => e.Booking.Guide.ApplicationUser
            ]);

            var totalCount = contacts.Count();
            var pages = Math.Ceiling(totalCount / 10.00);
            contacts = contacts.OrderByDescending(e => e.Id).Skip((page - 1) * 10).Take(10).ToList();

            AllContactAccessResponse data = new AllContactAccessResponse
            {
                ContactAccesses = contacts,
                CurrentPage = page,
                PagesNumber = pages
            };

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _contactAccessRepo.GetOneAsync(e => e.Id == id);
            if (contact == null) return NotFound();

            _contactAccessRepo.Delete(contact);
            await _contactAccessRepo.CommitAsync();
            TempData["success-notification"] = "Contact access record deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
