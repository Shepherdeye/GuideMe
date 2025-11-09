using GuideMe.Repositories;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuideMe.Areas.Profile.Controllers
{
    [Authorize]
    [Area(SD.ProfileArea)]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Visitor> _visitorRepository;
        private readonly IRepository<Guide> _guideRepository;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IRepository<Visitor> visitorRepository,
            IRepository<Guide> guideRepository
             )
        {
            _userManager = userManager;
            _visitorRepository = visitorRepository;
            _guideRepository = guideRepository;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            ProfileVm userProfile = user.Adapt<ProfileVm>();

            var guideData = await _guideRepository.GetOneAsync(e => e.ApplicationUserId == user.Id);
            var visitorData = await _visitorRepository.GetOneAsync(e => e.ApplicationUserId == user.Id);

            AllProfileData data = new AllProfileData()
            {
                ProfileVm = userProfile,

                // we  make a  new  class  here to  aviod null reference
                Guide = guideData ?? new Guide(), 
                Visitor = visitorData ?? new Visitor() 
            };

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileVm profileVm)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);

                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(profileVm.Id);

            if (user is null) return RedirectToAction("NotFoundPage", "Home", new { area = "Main" });

            user.FirstName = profileVm.FirstName;
            user.LastName = profileVm.LastName;
            user.Email = profileVm.Email;
            user.UserName = profileVm.UserName;
            user.Country = profileVm.Country;
            user.PhoneNumber = profileVm.PhoneNumber;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));


        }

        [HttpPost]

        public async Task<IActionResult> ChangePassword(ProfileChangePassword profileChangePassword)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);

                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(profileChangePassword.ApplicationUserId);

            if (user is null) return RedirectToAction("NotFoundPage", "Home", new { area = "Main" });

            var result = await _userManager.ChangePasswordAsync(user, profileChangePassword.CurrentPassword, profileChangePassword.NewPassword);

            if (!result.Succeeded)
            {
                TempData["error-notification"] = "Error with change password";

            }
            else
            {
                TempData["success-notification"] = "Password changed successfully";

            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));


        }


        // UpdateVisitorDetails

        [HttpPost]
        public async Task<IActionResult> UpdateVisitorDetails(VisitorDetailsVm visitorDetailsVm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);

                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return RedirectToAction(nameof(Index));
            }

            var visitor = await _visitorRepository.GetOneAsync(e => e.Id == visitorDetailsVm.Id);
            if (visitor is null)
            {
                return NotFound();
            }

            visitor.Id = visitorDetailsVm.Id;
            visitor.Passport = visitorDetailsVm.Passport;
            visitor.visitorStatus = (VisitorStatus)visitorDetailsVm.visitorStatus;

            _visitorRepository.Update(visitor);
            await _visitorRepository.CommitAsync();

            TempData["success-notification"] = "Visitor Details changed successfully";

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> UpdateGuideDetails(GuideDetailsVm guideDetailsVm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);

                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));

                return RedirectToAction(nameof(Index));
            }

            var guide = await _guideRepository.GetOneAsync(e => e.Id == guideDetailsVm.Id);

            if (guide is null)
            {
                return NotFound();
            }

            guide.Id = guideDetailsVm.Id;
            guide.LicenseNumber = guideDetailsVm.LicenseNumber;
            guide.YearsOfExperience = guideDetailsVm.YearsOfExperience;
            guide.NationalId = guideDetailsVm.NationalId;

            _guideRepository.Update(guide);
            await _guideRepository.CommitAsync();

            TempData["success-notification"] = "Guide Details changed successfully";

            return RedirectToAction(nameof(Index));


        }

        [HttpPost]

        public async Task<IActionResult> ChangePhoto(string userId, IFormFile ProfileImageFile)
        {

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound();

            var oldPath = "";

            if (user.ProfileImage is not null)
            {
                oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\identity\\profile", user.ProfileImage);
            }

            if (ProfileImageFile is not null && ProfileImageFile.Length > 0)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImageFile.FileName);
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\identity\\profile", filename);
                using (var stream = System.IO.File.Create(filepath))
                {
                    ProfileImageFile.CopyTo(stream);
                }

                user.ProfileImage = filename;
                await _userManager.UpdateAsync(user);


                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                TempData["success-notification"] = "profile image changed successfully";


                return RedirectToAction(nameof(Index));

            }
            TempData["error-notification"] = "Failed to change profile image";

            return RedirectToAction(nameof(Index));
        }

    }
}
