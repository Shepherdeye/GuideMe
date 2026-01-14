using GuideMe.Models;
using GuideMe.Repositories;
using GuideMe.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GuideMe.Utility;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    [Authorize(Roles = SD.AdminRole + "," + SD.SuperAdminRole)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var users = await _userManager.Users.ToListAsync();
            
            var totalCount = users.Count();
            var pages = Math.Ceiling(totalCount / 10.00);
            var pageination = users.OrderByDescending(u => u.Id).Skip((page - 1) * 10).Take(10).ToList();

            AllUsersResponse data = new AllUsersResponse
            {
                Users = pageination,
                CurrentPage = page,
                PagesNumber = pages
            };

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent deleting the current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["error-notification"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["success-notification"] = "User deleted successfully";
            }
            else
            {
                TempData["error-notification"] = "Failed to delete user";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            TempData["success-notification"] = "Email confirmed successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
