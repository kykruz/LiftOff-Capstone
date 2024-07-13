using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trips.Data;
using Trips.Models;

namespace Trips.Controllers
{
    [Authorize]
    public class ItineraryController : Controller
    {
        private readonly TripDbContext context;

        public ItineraryController(TripDbContext dbContext)
        {
            context = dbContext;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        public IActionResult Index()
        {
            var itineraries = new List<ItineraryViewModel>
            {
                new ItineraryViewModel
                {
                    Title = "Boat Trip",
                    Description = "Explore the beautiful canals of Venice.",
                    ImageUrl =
                        "https://upload.wikimedia.org/wikipedia/commons/1/17/Panorama_of_Canal_Grande_and_Ponte_di_Rialto%2C_Venice_-_September_2017.jpg"
                },
                new ItineraryViewModel
                {
                    Title = "Restaurant Trip",
                    Description = "Visit the iconic restaurants of Venice.",
                    ImageUrl =
                        "https://upload.wikimedia.org/wikipedia/commons/a/a1/Venice_Prosecco_and_Cicchetti.jpg"
                },
                new ItineraryViewModel
                {
                    Title = "Pub Trip",
                    Description = "Experience the bustling city life with some wine.",
                    ImageUrl =
                        "https://upload.wikimedia.org/wikipedia/commons/4/42/%22_05_-_ITALY_-_un_bacaro_a_Venezia_Osteria_appetizers_restaurant_in_Venice_wine_enoteca.jpg"
                }
            };

            return View(itineraries);
        }

        [HttpGet]
        public IActionResult PreMade()
        {
            List<Itinerary> itineraries = PreMadeItineraries.GetPreMadeItineraries();
            return View(itineraries);
        }

        [HttpPost]
        public async Task<IActionResult> PreMade(int[] selectedItineraries)
        {
            string userId = GetCurrentUserId();

            foreach (int itineraryId in selectedItineraries)
            {
                Itinerary preMadeItinerary = PreMadeItineraries
                    .GetPreMadeItineraries()
                    .FirstOrDefault(i => i.Id == itineraryId);

                if (preMadeItinerary != null)
                {
                    List<LocationData> selectedLocationDatas = preMadeItinerary.LocationDatas;

                    Itinerary itinerary = new Itinerary
                    {
                        Name = preMadeItinerary.Name,
                        UserId = userId,
                        Date = DateTime.UtcNow
                    };

                    foreach (LocationData locationData in selectedLocationDatas)
                    {
                        LocationData existingLocationData = context.LocationDatas.FirstOrDefault(
                            ld => ld.Id == locationData.Id
                        );

                        if (existingLocationData != null)
                        {
                            itinerary.ItineraryLocationDatas.Add(
                                new ItineraryLocationData
                                {
                                    LocationDataId = existingLocationData.Id
                                }
                            );
                        }
                    }

                    // Calculate total cost per person for selected locations
                    decimal totalCostPerPerson = (decimal)selectedLocationDatas.Sum(ld => ld.PricePerPerson);

                    // Calculate total cost per itinerary
                    decimal totalCostPerItinerary = totalCostPerPerson * itinerary.NumberOfPeople;

                    itinerary.TotalCostPerItinerary = totalCostPerItinerary; // Set total cost per itinerary

                    context.Itineraries.Add(itinerary);
                }
            }

            await context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateItineraryViewModel viewModel = new CreateItineraryViewModel();

            viewModel.AvailableCategories = context
                .LocationDatas.Select(ld => ld.Category)
                .Distinct()
                .ToList();

            viewModel.AvailableLocations = context.LocationDatas.ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateItineraryViewModel createItineraryViewModel, int numberOfPets)
        {
            if (ModelState.IsValid)
            {
                string userId = GetCurrentUserId();

                // Retrieve selected s based on user's selection
                List<LocationData> selectedLocationDatas = await context
                    .LocationDatas.Where(ld =>
                        createItineraryViewModel.SelectedLocationIds.Contains(ld.Id)
                        && createItineraryViewModel.SelectedCategories.Contains(ld.Category)
                    )
                    .ToListAsync();

                // Create new Itinerary object
                Itinerary itinerary = new Itinerary
                {
                    Name = createItineraryViewModel.Name,
                    UserId = userId,
                    ItineraryLocationDatas = selectedLocationDatas
                        .Select(ld => new ItineraryLocationData { LocationData = ld })
                        .ToList(),
                    Date = createItineraryViewModel.Date.Date,
                    NumberOfPeople = createItineraryViewModel.NumberOfPeople, // Set the number of people here
                    NumberOfPets = numberOfPets // Set the number of pets here
                };

                // Calculate total cost per person for selected locations
                decimal totalCostPerPerson = (decimal)selectedLocationDatas.Sum(ld => ld.PricePerPerson);

                // Calculate total cost per itinerary
                decimal totalCostPerItinerary = totalCostPerPerson * itinerary.NumberOfPeople;

                itinerary.TotalCostPerItinerary = totalCostPerItinerary; // Set total cost per itinerary

                // Add itinerary to context and save changes
                context.Itineraries.Add(itinerary);
                await context.SaveChangesAsync();

                return RedirectToAction("Success");
            }

            // If ModelState is not valid, re-populate view model and return the view with errors
            createItineraryViewModel.AvailableCategories = context
                .LocationDatas.Select(ld => ld.Category)
                .Distinct()
                .ToList();
            createItineraryViewModel.AvailableLocations = context.LocationDatas.ToList();

            return View(createItineraryViewModel);
        }

        public async Task<IActionResult> Success()
        {
            string userId = GetCurrentUserId();

            List<Itinerary> itineraries = await context
                .Itineraries.Where(i => i.UserId == userId)
                .Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .ToListAsync();

            return View(itineraries);
        }

        public async Task<IActionResult> ViewLocations(int itineraryId)
        {
            string userId = GetCurrentUserId();

            Itinerary itinerary = await context
                .Itineraries.Where(i => i.UserId == userId && i.Id == itineraryId)
                .Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .FirstOrDefaultAsync();

            if (itinerary == null)
            {
                return NotFound();
            }

            return View(itinerary);
        }

        [HttpGet]
        public IActionResult Delete()
        {
            string userId = GetCurrentUserId();

            List<Itinerary> itineraries = context
                .Itineraries.Where(i => i.UserId == userId)
                .ToList();

            return View("Delete", itineraries);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int[] ItineraryIds)
        {
            foreach (int id in ItineraryIds)
            {
                Itinerary? theItinerary = await context.Itineraries.FindAsync(id);
                if (theItinerary != null)
                {
                    context.Itineraries.Remove(theItinerary);
                }
            }
            await context.SaveChangesAsync();

            string userId = GetCurrentUserId();
            List<Itinerary> itineraries = await context
                .Itineraries.Where(i => i.UserId == userId)
                .ToListAsync();

            return View("Delete", itineraries);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int itineraryId)
        {
            string userId = GetCurrentUserId();

            Itinerary itinerary = await context
                .Itineraries.Where(i => i.UserId == userId && i.Id == itineraryId)
                .Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .FirstOrDefaultAsync();

            if (itinerary == null)
            {
                return NotFound();
            }

            EditItineraryViewModel viewModel = new EditItineraryViewModel
            {
                ItineraryId = itinerary.Id,
                Name = itinerary.Name,
                Date = itinerary.Date,
                SelectedLocationIds = itinerary
                    .ItineraryLocationDatas.Select(il => il.LocationDataId)
                    .ToList(),
                AvailableCategories = context
                    .LocationDatas.Select(ld => ld.Category)
                    .Distinct()
                    .ToList(),
                AvailableLocations = context.LocationDatas.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditItineraryViewModel editViewModel)
        {
            if (ModelState.IsValid)
            {
                string userId = GetCurrentUserId();

                Itinerary itinerary = await context
                    .Itineraries.Include(i => i.ItineraryLocationDatas)
                    .FirstOrDefaultAsync(i =>
                        i.Id == editViewModel.ItineraryId && i.UserId == userId
                    );

                if (itinerary == null)
                {
                    return NotFound();
                }

                itinerary.Name = editViewModel.Name;
                itinerary.Date = editViewModel.Date.Date;

                itinerary.ItineraryLocationDatas.Clear();

                IQueryable<LocationData> locationDatasQuery = context.LocationDatas.AsQueryable();

                if (
                    editViewModel.SelectedCategories != null
                    && editViewModel.SelectedCategories.Any()
                )
                {
                    locationDatasQuery = locationDatasQuery.Where(ld =>
                        editViewModel.SelectedCategories.Contains(ld.Category)
                    );
                }

                List<LocationData> selectedLocationDatas = await locationDatasQuery
                    .Where(ld => editViewModel.SelectedLocationIds.Contains(ld.Id))
                    .ToListAsync();

                foreach (LocationData locationData in selectedLocationDatas)
                {
                    itinerary.ItineraryLocationDatas.Add(
                        new ItineraryLocationData { LocationDataId = locationData.Id }
                    );
                }

                // Calculate total cost per person for selected locations
                decimal totalCostPerPerson = (decimal)selectedLocationDatas.Sum(ld => ld.PricePerPerson);

                // Calculate total cost per itinerary
                decimal totalCostPerItinerary = totalCostPerPerson * itinerary.NumberOfPeople;

                itinerary.TotalCostPerItinerary = totalCostPerItinerary; // Set total cost per itinerary

                await context.SaveChangesAsync();

                return RedirectToAction("ViewLocations", new { itineraryId = itinerary.Id });
            }

            editViewModel.AvailableCategories = context
                .LocationDatas.Select(ld => ld.Category)
                .Distinct()
                .ToList();
            editViewModel.AvailableLocations = context.LocationDatas.ToList();

            return View(editViewModel);
        }

        [HttpPost]
        public IActionResult CalculateTotalCost(int itineraryId, int numberOfPeople)
        {
            string userId = GetCurrentUserId();

            var itinerary = context
                .Itineraries.Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .FirstOrDefault(i => i.UserId == userId && i.Id == itineraryId);

            if (itinerary == null)
            {
                return NotFound();
            }

            decimal totalCostForAllLocations = CalculateTotalCostForLocations(itinerary);

            decimal totalCostForAllPeople = totalCostForAllLocations * numberOfPeople;

            itinerary.TotalCostForAllLocations = totalCostForAllLocations;
            itinerary.TotalCostForAllPeople = totalCostForAllPeople;
            itinerary.NumberOfPeople = numberOfPeople;

            context.SaveChanges();

            return View("ViewLocations", itinerary);
        }

        private decimal CalculateTotalCostForLocations(Itinerary itinerary)
        {
            return (decimal)
                itinerary.ItineraryLocationDatas.Sum(il => (double)il.LocationData.PricePerPerson);
        }
    }
}
