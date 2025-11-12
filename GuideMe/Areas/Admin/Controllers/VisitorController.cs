using GuideMe.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    public class VisitorController : Controller
    {
        private readonly IRepository<Visitor> _visitorRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public VisitorController(IRepository<Visitor> visitorRepo, UserManager<ApplicationUser> userManager)
        {
            _visitorRepo = visitorRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery]int page = 1)
        {
            var response = new List<VisitorResponseVM>();

            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {

                var visitor = await _visitorRepo.GetOneAsync(e => e.ApplicationUserId == user.Id);

                if (visitor == null)
                    continue;


                var userVM = new VisitorResponseVM
                {
                    Id = visitor.Id,
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
                    Passport = visitor.Passport,
                    visitorStatus = visitor.visitorStatus

                };

                response.Add(userVM);
            }

            var totalcount = response.Count();
            double pages = Math.Ceiling(totalcount / 8.00);
            var pageination = response.Skip((page - 1) * 8).Take(8).ToList();

            AllVisitorResponse returnedData = new AllVisitorResponse
            {
                Visitors = pageination,
                PagesNumber = pages,
                TotalCount = totalcount,
                CurrentPage=page

            };


            return View(returnedData);
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(VisitorResponseVM visitorResponseVM, IFormFile? ProfileImage)
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

                FirstName = visitorResponseVM.FirstName,
                LastName = visitorResponseVM.LastName,
                UserName = visitorResponseVM.UserName,
                Email = visitorResponseVM.Email,
                PhoneNumber = visitorResponseVM.PhoneNumber,
                Country = visitorResponseVM.Country,
                Gender = visitorResponseVM.Gender,
                EmailConfirmed = visitorResponseVM.EmailConfirmed,
                ProfileImage = imgesrc,
                Role = UserRole.Visitor
            };

            var result = await _userManager.CreateAsync(user, visitorResponseVM.Password);


            if (!result.Succeeded)
            {
                TempData["error-notification"] = "Faild To Create User,";
                return View(visitorResponseVM);
            }

            //add role visitor
            await _userManager.AddToRoleAsync(user, SD.VisitorRole);

            var visitor = new Visitor
            {

                ApplicationUserId = user.Id,
                Passport = visitorResponseVM.Passport,
                visitorStatus = visitorResponseVM.visitorStatus

            };

            await _visitorRepo.CreateAsync(visitor);
            await _visitorRepo.CommitAsync();




            TempData["success-notification"] = "created successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var visitor = await _visitorRepo.GetOneAsync(e => e.Id == id);

            if (visitor == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var user = await _userManager.FindByIdAsync(visitor.ApplicationUserId);

            if (user == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var userVM = new VisitorEditVM
            {
                Id = visitor.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Country = user.Country,
                EmailConfirmed = user.EmailConfirmed,
                ProfileImage = user.ProfileImage,
                Passport = visitor.Passport,
                visitorStatus = visitor.visitorStatus

            };


            return View(userVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VisitorEditVM visitorEditVM, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors);
                TempData["error-notification"] = string.Join(", ", errors.Select(e => e.ErrorMessage));
                return View(visitorEditVM);
            }

            var visitor = await _visitorRepo.GetOneAsync(e => e.Id == visitorEditVM.Id);

            if (visitor == null) return RedirectToAction("NotFoundPage", "Home", new { area = "Admin" });

            var user = await _userManager.FindByIdAsync(visitor.ApplicationUserId);

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

                var oldpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\identity\\profile", oldImagesrc);

                if (System.IO.File.Exists(oldpath))
                {
                    System.IO.File.Delete(oldpath);
                }
            }
            else
            {
                user.ProfileImage = oldImagesrc;
            }

            user.FirstName = visitorEditVM.FirstName;
            user.LastName = visitorEditVM.LastName;
            user.UserName = visitorEditVM.UserName;
            user.Email = visitorEditVM.Email;
            user.EmailConfirmed = visitorEditVM.EmailConfirmed;
            user.PhoneNumber = visitorEditVM.PhoneNumber;
            user.Country = visitorEditVM.Country;
            user.Gender = visitorEditVM.Gender;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }

                return View(visitorEditVM);
            }

            visitor.Passport = visitorEditVM.Passport;
            visitor.visitorStatus = visitorEditVM.visitorStatus;

            _visitorRepo.Update(visitor);
            await _visitorRepo.CommitAsync();

            TempData["success-notification"] = "Update Visitor SuccessFully";

            return RedirectToAction(nameof(Index));


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var visitor = await _visitorRepo.GetOneAsync(e => e.Id == id);
            if (visitor == null)
            {

                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(visitor.ApplicationUserId);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));

        }

    }
}
