using GuideMe.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class GuideController : Controller
    {
        private readonly IRepository<Guide> _guideRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public GuideController(IRepository<Guide> guideRepo, UserManager<ApplicationUser> userManager)
        {
            _guideRepo = guideRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1)
        {
            var response = new List<GuideResponseVM>();

            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {

                var guide = await _guideRepo.GetOneAsync(e => e.ApplicationUserId == user.Id);

                if (guide == null)
                    continue;


                var userVM = new GuideResponseVM
                {
                    Id = guide.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    Country = user.Country,
                    EmailConfirmed = user.EmailConfirmed,
                    Role = user.Role,
                    ProfileImage = user.ProfileImage,
                    LicenseNumber = guide.LicenseNumber,
                    NationalId = guide.NationalId,
                    YearsOfExperience = guide.YearsOfExperience

                };

                response.Add(userVM);
            }

            var totalcount = response.Count();
            double pages = Math.Ceiling(totalcount / 15.00);
            var pageination = response.Skip((page - 1) * 15).Take(15).ToList();

            AllGuideResponse returnedData = new AllGuideResponse
            {
                Guides = pageination,
                PagesNumber = pages,
                CurrentPage = page

            };


            return View(returnedData);
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(GuideResponseVM guideResponseVM, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View();
            }


            var imgesrc = "";

            if (ProfileImage is not null && ProfileImage.Length > 0)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\identity\\profile", filename);

                using (var stream = System.IO.File.Create(filePath))
                {
                    ProfileImage.CopyTo(stream);
                }

                imgesrc = filename;
            }



            var user = new ApplicationUser
            {

                FirstName = guideResponseVM.FirstName,
                LastName = guideResponseVM.LastName,
                UserName = guideResponseVM.UserName,
                Email = guideResponseVM.Email,
                PhoneNumber = guideResponseVM.PhoneNumber,
                Country = guideResponseVM.Country,
                Gender = guideResponseVM.Gender,
                EmailConfirmed = guideResponseVM.EmailConfirmed,
                ProfileImage = imgesrc,
                Role = UserRole.Guide
            };

            var result = await _userManager.CreateAsync(user, guideResponseVM.Password);


            if (!result.Succeeded)
            {
                TempData["error-notification"] = "Faild To Create User,";
                return View(guideResponseVM);
            }

            //add role guide
            await _userManager.AddToRoleAsync(user, SD.GuideRole);

            var guide = new Guide
            {

                ApplicationUserId = user.Id,
                LicenseNumber = guideResponseVM.LicenseNumber,
                NationalId = guideResponseVM.NationalId,
                YearsOfExperience = guideResponseVM.YearsOfExperience

            };

            await _guideRepo.CreateAsync(guide);
            await _guideRepo.CommitAsync();




            TempData["success-notification"] = "created successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var guide = await _guideRepo.GetOneAsync(e => e.Id == id);

            if (guide == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var user = await _userManager.FindByIdAsync(guide.ApplicationUserId);

            if (user == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var userVM = new GuideEditVM
            {
                Id = guide.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Country = user.Country,
                EmailConfirmed = user.EmailConfirmed,
                ProfileImage = user.ProfileImage,
                LicenseNumber = guide.LicenseNumber,
                NationalId = guide.NationalId,
                YearsOfExperience = guide.YearsOfExperience

            };


            return View(userVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GuideEditVM guideEditVM, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(guideEditVM);
            }

            var guide = await _guideRepo.GetOneAsync(e => e.Id == guideEditVM.Id);

            if (guide == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var user = await _userManager.FindByIdAsync(guide.ApplicationUserId);

            if (user == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var oldImagesrc = user.ProfileImage;

            if (ProfileImage is not null && ProfileImage.Length > 0)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\identity\\profile", filename);
                using (var stream = System.IO.File.Create(filePath))
                {
                    ProfileImage.CopyTo(stream);
                }

                user.ProfileImage = filename;

                if(oldImagesrc is not null &&  oldImagesrc != "")
                {
                    var oldpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\identity\\profile", oldImagesrc);

                    if (System.IO.File.Exists(oldpath))
                    {
                        System.IO.File.Delete(oldpath);
                    }
                }


            }
            else
            {
                user.ProfileImage = oldImagesrc;
            }

            user.FirstName = guideEditVM.FirstName;
            user.LastName = guideEditVM.LastName;
            user.UserName = guideEditVM.UserName;
            user.Email = guideEditVM.Email;
            user.EmailConfirmed = guideEditVM.EmailConfirmed;
            user.PhoneNumber = guideEditVM.PhoneNumber;
            user.Country = guideEditVM.Country;
            user.Gender = guideEditVM.Gender;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }

                return View(guideEditVM);
            }

            guide.LicenseNumber = guideEditVM.LicenseNumber;
            guide.NationalId = guideEditVM.NationalId;
            guide.YearsOfExperience = guideEditVM.YearsOfExperience;

            _guideRepo.Update(guide);
            await _guideRepo.CommitAsync();

            TempData["success-notification"] = "Update Guide SuccessFully";

            return RedirectToAction(nameof(Index));


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var guide = await _guideRepo.GetOneAsync(e => e.Id == id);
            if (guide == null)
            {

                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(guide.ApplicationUserId);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));

        }
    }
}
