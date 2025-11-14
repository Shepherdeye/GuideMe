using GuideMe.Models;
using GuideMe.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class OfferController : Controller
    {
        private readonly IRepository<Offer> _offerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Trip> _tripRepo;
        private readonly IRepository<Guide> _guideRepo;

        public OfferController(
            IRepository<Offer> offerRepository
            , UserManager<ApplicationUser> userManager
            , IRepository<Trip> tripRepo
            , IRepository<Guide> guideRepo
            )
        {
            _offerRepository = offerRepository;
            _userManager = userManager;
            _tripRepo = tripRepo;
            _guideRepo = guideRepo;
        }

        public async Task<IActionResult> Index(int page = 1)
        {


            Expression<Func<Offer, object>>[] includes =
            {
                o => o.Guide,
                o => o.Guide.ApplicationUser,
                o => o.Trip
            };

            var offers = await _offerRepository.GetAsync(includes: includes);

            var totaCount = offers.Count();
            var pages = Math.Ceiling(totaCount / 10.00);
            offers = offers.Skip((page - 1)).Take(10).ToList();


            AllOffersResponse data = new AllOffersResponse()
            {
                Offers = offers,
                CurrentPage = page,
                PagesNumber = pages

            };

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.Role == UserRole.Visitor)
            {
                TempData["error-notification"] = "Only Admin & Guides  Can Create Offers";
                return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });
            }

            var guide = await _guideRepo.GetOneAsync(e => e.ApplicationUserId == user.Id);

            if (guide is null)
            {
                TempData["error-notification"] = "User is not registered as a Guide";
                return RedirectToAction("Index");
            }

            Offer offer = new Offer();

            offer.GuideId = guide.Id;


            // كل المستخدمين سيختارون Trip من ViewBag
            var trips = await _tripRepo.GetAsync();
            ViewBag.Trips = new SelectList(trips, "Id", "Title");

            return View(offer);
        }

        [HttpPost]

        public async Task<IActionResult> Create(Offer offer)
        {


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                var trips = await _tripRepo.GetAsync();
                
                ViewBag.Trips = new SelectList(trips, "Id", "Title");

                
                return View(offer);
            }

            await _offerRepository.CreateAsync(offer);
            await _offerRepository.CommitAsync();


            TempData["success-notification"] = "Offer Created Successfully";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]

        public async Task<IActionResult> Edit(int id)
        {

            var offer = await _offerRepository.GetOneAsync(e=>e.Id==id);
            if (offer is null)
            {

                TempData["error-notification"] = "Offer Not Found";
                return RedirectToAction("NotFoudPage", "Home", new { area="Admin"});

            }
            // Get trips for dropdown
            var trips = await _tripRepo.GetAsync();
            ViewBag.Trips = new SelectList(trips, "Id", "Title", offer.TripId);

            return View(offer);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Offer offer)
        {
            if (!ModelState.IsValid)
            {
                var trips = await _tripRepo.GetAsync();
                ViewBag.Trips = new SelectList(trips, "Id", "Title", offer.TripId);
                return View(offer);
            }

            _offerRepository.Update(offer);
            await _offerRepository.CommitAsync();

            TempData["success-notification"] = "Offer updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id) {

            var offer = await _offerRepository.GetOneAsync(e => e.Id == id);
            if (offer is null)
            {
                TempData["error-notification"] = "Offer Not Found";
                return RedirectToAction("NotFoudPage", "Home", new { area = "Admin" });
            }
            _offerRepository.Delete(offer);
            await _offerRepository.CommitAsync();

            TempData["success-notification"] = "Offer Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
