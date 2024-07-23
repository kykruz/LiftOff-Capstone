using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    public class QuickTourController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new QuickTourViewModel
            {
                VideoUrl = "https://www.youtube.com/embed/JphHw6iU4m8",
                Title = "Experience Venice’s Spectacular Beauty in Under 4 Minutes | Short Film Showcase"
            };
            return View(viewModel);
        }
    }
}
