using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Trips.Data;
using Trips.Models;
using Trips.ViewModels;

namespace Trips.Controllers
{
    [Route("QuickTour")]
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
