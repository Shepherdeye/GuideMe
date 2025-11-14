using GuideMe.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Main.Controllers
{
    [Area(SD.MainArea)]
    public class TripController : Controller
    {
        private readonly IRepository<Trip> _tripRepo;

        public TripController(IRepository<Trip> tripRepo)
        {
            _tripRepo = tripRepo;
        }
        public IActionResult Index()
        {

            return View();
        }
    }
}
