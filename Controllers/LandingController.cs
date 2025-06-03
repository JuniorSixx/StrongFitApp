using Microsoft.AspNetCore.Mvc;

namespace StrongFitApp.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Layout"] = "_LandingLayout";
            return View();
        }

        public IActionResult Portal()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
