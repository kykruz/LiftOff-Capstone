using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Exchange.Services;
using Itinerary_Designer.Models;
using Microsoft.AspNetCore.Mvc;
using Trips.Models;
using Trips.ViewModels;

namespace Trips.Controllers;


public class HomeController : Controller
{
    private readonly WeatherService _weatherService;
    private readonly ExchangeRatesApiService _exchangeRatesApi;

    public HomeController()
    {
        _weatherService = new WeatherService();
    }

    public async Task<IActionResult> Index()
    {
        var userViewModel = new UserViewModel
        {
            Username = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest"
        };

        string placeId = "venice"; 
        string apiKey = "gyfpqgn0mw1m83g5pm4mdgulqbh13asty8o4rwva";

        string weatherData = await _weatherService.GetWeatherAsync(placeId, apiKey);

        ViewData["WeatherData"] = weatherData;

        return View(userViewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}