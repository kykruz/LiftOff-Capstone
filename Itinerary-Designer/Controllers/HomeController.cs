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


    //Action for the Currency Conversion
    [HttpPost]
    public async Task<IActionResult> CovertCurrency(
        string fromCurrency,
        string toCurrency,
        double amount
    )
    {
        try
        {
            Exchange.Services.ConvertRequest request = new Exchange.Services.ConvertRequest(fromCurrency, toCurrency, amount);
            // Exchange.Services.ConvertResponse response = await _exchangeRatesApi.ConvertAsync(request);

            // Preparing data to pass to the View
            ViewData["Amount"] = amount;
            ViewData["FromCurrency"] = fromCurrency;
            ViewData["ToCurrency"] = toCurrency;
            // ViewData["ConvertedAmount"] = response.Result;

            return View("Index");
        }
        catch (Exception ex)
        {
            ViewData["Error"] = ex.Message;
            return View("Index");
        }
    }

}
