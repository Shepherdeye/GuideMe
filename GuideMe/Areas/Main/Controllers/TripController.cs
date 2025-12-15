using GuideMe.Repositories;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Areas.Main.Controllers
{
    [Area(SD.MainArea)]
    public class TripController : Controller
    {
        private readonly IRepository<Trip> _tripRepo;
        private readonly IRepository<Offer> _offerRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly ApplicationDbContext _context;

        public TripController(
              IRepository<Trip> tripRepo
            , IRepository<Offer> offerRepo
            , IRepository<Review> ReviewRepo
            , UserManager<ApplicationUser> userManager
            , IRepository<Booking> bookingRepo
            , ApplicationDbContext context
            )
        {
            _tripRepo = tripRepo;
            _offerRepo = offerRepo;
            _reviewRepo = ReviewRepo;
            _userManager = userManager;
            _bookingRepo = bookingRepo;
            _context = context;
        }
        public async Task<IActionResult> Index(TripFilterVM filter, int page = 1)
        {
            var trips = await _tripRepo.GetAsync(includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);

            if (!string.IsNullOrEmpty(filter.Title))
            {

                trips = trips.Where(e => e.Title.Contains(filter.Title)).ToList();
            }

            if (!string.IsNullOrEmpty(filter.Distination))
            {
                trips = trips.Where(e => e.Destination.Contains(filter.Distination)).ToList();
            }

            if (filter.MinPrice is not null && filter.MinPrice != 0)
            {
                trips = trips.Where(e => e.Price >= filter.MinPrice).ToList();
            }

            if (filter.MaxPrice is not null && filter.MaxPrice != 0)
            {
                trips = trips.Where(e => e.Price <= filter.MaxPrice).ToList();
            }

            if (filter.Active == true)
            {
                trips = trips.Where(e => e.Status == TripStatus.Open).ToList();
            }

            var totalcount = trips.Count();
            var totalpages = Math.Ceiling(totalcount / 6.00);
            trips = trips.Skip((page - 1) * 6).Take(6).ToList();


            //get the current user
            var userId = _userManager.GetUserId(User);

            var currentUser = await _context.Users
                .Include(u => u.Visitor)
                .Include(u => u.Guide)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
                return NotFound();





            TripsWithFilterVm Data = new TripsWithFilterVm()
            {
                PagesNumber = totalpages,
                CurrentPage = page,
                Trips = trips,
                Filter = filter,
                CurrentUser = currentUser

            };

            return View(Data);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id, [FromQuery] int activeReviewId = 0, int activeOfferId = 0)
        {
            var trip = await _tripRepo.GetOneAsync(e => e.Id == id, includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);
            if (trip == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Main" });
            var offers = await _offerRepo.GetAsync(e => e.TripId == trip.Id, includes: [e => e.Guide, e => e.Guide.ApplicationUser]);
            var reviews = await _reviewRepo.GetAsync(e => e.TripId == trip.Id, includes: [e => e.Visitor, e => e.Visitor.ApplicationUser]);



            //get user 

            var userId = _userManager.GetUserId(User);

            var currentUser = await _context.Users
                .Include(u => u.Visitor)
                .Include(u => u.Guide)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
                return NotFound();


            var activeReview = await _reviewRepo.GetOneAsync(e => e.Id == activeReviewId);

            if (activeReviewId > 0 && currentUser?.Visitor?.Id != activeReview?.VisitorId)
            {
                return RedirectToAction("Details", new { id = trip.Id });
            }

            var activeOffer = await _offerRepo.GetOneAsync(e => e.Id == activeOfferId);

            if (activeOfferId > 0 && currentUser?.Guide?.Id != activeOffer?.GuideId)
            {
                return RedirectToAction("Details", new { id = trip.Id });
            }

            TripDetailsResponseVM data = new TripDetailsResponseVM()
            {
                Trip = trip,
                Offers = offers,
                Reviews = reviews,
                CurrentUser = currentUser,
                ActiveReview = activeReview,
                ActiveOffer = activeOffer

            };


            return View(data);
        }



        ///Start of  the review Crud

        [HttpPost]
        public async Task<IActionResult> CreateReview(CreateReviewVM reviewVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(",", errors.Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = reviewVM.TripId });
            }


            var review = reviewVM.Adapt<Review>();

            review.ReviewDate = DateTime.UtcNow;

            await _reviewRepo.CreateAsync(review);
            await _reviewRepo.CommitAsync();

            TempData["success-notification"] = "Your review has been submitted successfully!";
            return RedirectToAction(nameof(Details), new { id = reviewVM.TripId });
        }

        [HttpPost]
        public async Task<IActionResult> EditReview(EditReviewVm editReviewVm)
        {

            if (!ModelState.IsValid)
            {

                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(",", errors.Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = editReviewVm.TripId });
            }


            var review = await _reviewRepo.GetOneAsync(e => e.Id == editReviewVm.ReviewId);
            if (review == null)
            {
                TempData["error-notification"] = "Review Not Founded";

                return RedirectToAction(nameof(Details), new { id = editReviewVm.TripId });

            }

            review.Id = editReviewVm.ReviewId;
            review.Id = editReviewVm.ReviewId;
            review.Comment = editReviewVm.Comment;
            review.RatingReview = editReviewVm.RatingReview;
            review.ReviewDate = DateTime.UtcNow;

            _reviewRepo.Update(review);
            await _reviewRepo.CommitAsync();


            TempData["success-notification"] = "Your review has been Updated successfully!";
            return RedirectToAction(nameof(Details), new { id = editReviewVm.TripId });

        }


        [HttpPost]
        public async Task<IActionResult> DeleteReview(int ReviewId, int TripId)
        {
            var review = await _reviewRepo.GetOneAsync(e => e.Id == ReviewId);
            if (review is null)
            {
                TempData["error-notification"] = "Review Not Founded";

                return RedirectToAction(nameof(Details), new { id = TripId });
            }

            _reviewRepo.Delete(review);
            await _reviewRepo.CommitAsync();


            TempData["success-notification"] = "Your review has been Deleted successfully!";
            return RedirectToAction(nameof(Details), new { id = TripId });

        }

        ///End of  the review Crud



        //start the offer crud

        [HttpPost]
        public async Task<IActionResult> CreateOffer(OfferCreateVM offerCreateVM)
        {
            if (!ModelState.IsValid)
            {

                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(",", errors.Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = offerCreateVM.TripId });
            }

            var offer = offerCreateVM.Adapt<Offer>();

            await _offerRepo.CreateAsync(offer);
            await _offerRepo.CommitAsync();

            TempData["success-notification"] = "Your Offer has been Added successfully!";
            return RedirectToAction(nameof(Details), new { id = offerCreateVM.TripId });
        }


        [HttpPost]
        public async Task<IActionResult> EditOffer(EditOfferVm editOfferVm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(",", errors.Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = editOfferVm.TripId });
            }

            var offer = await _offerRepo.GetOneAsync(e => e.Id == editOfferVm.Id);

            if (offer is null)
            {
                TempData["error-notification"] = "Offer Not Founded";

                return RedirectToAction(nameof(Details), new { id = editOfferVm.TripId });
            }
            offer.Id = editOfferVm.Id;
            offer.Message = editOfferVm.Message;
            offer.OfferedPrice = editOfferVm.OfferedPrice;
            offer.OfferStartDate = editOfferVm.OfferStartDate;
            offer.OfferEndDate = editOfferVm.OfferEndDate;

            _offerRepo.Update(offer);
            await _offerRepo.CommitAsync();

            TempData["success-notification"] = "Your Offer has been Updated successfully!";
            return RedirectToAction(nameof(Details), new { id = editOfferVm.TripId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOffer(int offerId)
        {
            var offer = await _offerRepo.GetOneAsync(e => e.Id == offerId);
            if (offer is null)
            {
                TempData["error-notification"] = "Offer Not Founded";

                return RedirectToAction(nameof(Index));
            }

            _offerRepo.Delete(offer);
            await _offerRepo.CommitAsync();

            TempData["success-notification"] = "Your Offer has been Deleted successfully!";
            return RedirectToAction(nameof(Details), new { id = offer.TripId });
        }

        //End the offer crud


        // start  crud  of the booking

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingVM createBookingVM)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(",", errors.Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = createBookingVM.TripId });
            }

            //1.create a new Booking

            var booking = createBookingVM.Adapt<Booking>();

            await _bookingRepo.CreateAsync(booking);
            await _bookingRepo.CommitAsync();

            //2. change the offer status  to be accepted

            var takenOffer = await _offerRepo.GetOneAsync(e => e.Id == createBookingVM.OfferId);
            if (takenOffer is null) return NotFound();
            takenOffer.Status = OfferStatus.Accepted;

            _offerRepo.Update(takenOffer);
            await _offerRepo.CommitAsync();


            //3.all other offer shoud be Rejected

            var rejectedOffers = await _offerRepo.GetAsync(e => e.Id != createBookingVM.OfferId && e.TripId == createBookingVM.TripId);

            foreach (var offer in rejectedOffers)
            {
                offer.Status = OfferStatus.Rejected;
                _offerRepo.Update(offer);
                await _offerRepo.CommitAsync();

            }

            //4.change the status of the trip to  be close

            var trip = await _tripRepo.GetOneAsync(e => e.Id == createBookingVM.TripId);
            if (trip is null) return NotFound();
            trip.Status = TripStatus.Close;
            _tripRepo.Update(trip);
            await _tripRepo.CommitAsync();



            TempData["success-notification"] = "Your Booking has been created Successfully wait for the Guide Approval";
            return RedirectToAction(nameof(Details), new { id = createBookingVM.TripId });
        }


        // End  crud  of the booking



    }
}
