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
        public async Task<IActionResult> Index()
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
                    Role = user.Role,
                    ProfileImage = user.ProfileImage,
                    Passport = visitor.Passport,
                    visitorStatus = visitor.visitorStatus
                };

                response.Add(userVM);
            }

            return View(response);
        }




    }
}
