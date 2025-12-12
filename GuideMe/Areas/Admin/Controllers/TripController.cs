using GuideMe.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class TripController : Controller
    {
        private readonly IRepository<Trip> _tripRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Visitor> _visitorRepo;

        public TripController(
            IRepository<Trip> tripRepo,
            UserManager<ApplicationUser> userManager
            , IRepository<Visitor> visitorRepo
            )
        {
            _tripRepo = tripRepo;
            _userManager = userManager;
            _visitorRepo = visitorRepo;
        }
        public async Task<IActionResult> Index(int page = 1)
        {

            var trips = await _tripRepo.GetAsync(includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);

            var totalCount = trips.Count();
            double pagesNumber = Math.Ceiling(totalCount / 10.00);

            trips = trips.Skip((page - 1) * 10).Take(10).ToList();

            AllTripData data = new AllTripData
            {
                Trips = trips,
                CurrentPage = page,
                PagesNumber = pagesNumber
            };

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.Role == UserRole.Guide)
            {
                TempData["error-notification"] = "Only Admin and Visitor can Create Trip";
                return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });
            }

            var visitor = await _visitorRepo.GetOneAsync(e => e.ApplicationUserId == user.Id);

            if (visitor is null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            return View(new Trip { VisitorId = visitor.Id });

        }


        [HttpPost]
        public async Task<IActionResult> Create(Trip trip, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);

                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(trip);
            }

            var filename = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\main\\img", filename);

            using (var stream = System.IO.File.Create(filepath))
            {
                await Image.CopyToAsync(stream);
            }
            trip.Image = filename;


            await _tripRepo.CreateAsync(trip);
            await _tripRepo.CommitAsync();
            TempData["success-notification"] = "Trip Created successfully";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var trip = await _tripRepo.GetOneAsync(e => e.Id == id);
            if (trip is null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });


            return View(trip);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Trip trip, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);

                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(trip);
            }
            var tripDB = await _tripRepo.GetOneAsync(e => e.Id == trip.Id, tracked: false);

            if (tripDB is null) return NotFound();

            if (Image is not null && Image.Length > 0)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\main\\img", filename);

                using (var stream = System.IO.File.Create(filepath))
                {
                    await Image.CopyToAsync(stream);
                }

                trip.Image = filename;

                var oldFilepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\main\\img", tripDB.Image);
                if (System.IO.File.Exists(oldFilepath))
                {
                    System.IO.File.Delete(oldFilepath);
                }
            }
            else
            {
                trip.Image = tripDB.Image;
            }


            _tripRepo.Update(trip);
            await _tripRepo.CommitAsync();

            TempData["success-notification"] = "Trip Updated successfully";

            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> Delete([FromRoute] int id)
        {

            var trip = await _tripRepo.GetOneAsync(e => e.Id == id);
            if (trip == null) return NotFound();

            _tripRepo.Delete(trip);
            await _tripRepo.CommitAsync();
            TempData["success-notification"] = "Trip Deleted successfully";


            return RedirectToAction(nameof(Index));

        }








    }
}
