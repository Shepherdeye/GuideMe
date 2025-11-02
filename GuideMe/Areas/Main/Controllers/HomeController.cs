using Microsoft.AspNetCore.Mvc;

namespace GuideMe.Areas.Main.Controllers
{
    [Area(SD.MainArea)]
    public class HomeController : Controller
    {

    

        public IActionResult Index()
        {
            return View();
        }

       public IActionResult NotFoundPage()
        {

            return View();
        }
     
    }
}
