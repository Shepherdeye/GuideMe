using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Profile.Controllers
{
    [Authorize]
    [Area(SD.ProfileArea)]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);

            ProfileVm userProfile = user.Adapt<ProfileVm>();

            if (userProfile is null) return NotFound();


            return View(userProfile);
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

    }
}
