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
    // The [Authorize] attribute ensures that only authenticated users can access the actions in this controller
    [Authorize]
    public class ItineraryController : Controller
    {
        // Read-only field to hold the database context for data access
        private readonly TripDbContext context;

        // Constructor that initializes the database context
        public ItineraryController(TripDbContext dbContext)
        {
            context = dbContext;
        }

        // Private method to get the current user's ID from the claims principal
        private string GetCurrentUserId()
        {
            // Retrieves the user ID from the authenticated user's identity claims
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // Action method to display a list of itineraries (e.g., a homepage for itineraries)
        public IActionResult Index()
        {
            // Create a list of ItineraryViewModel objects with pre-defined data for demonstration
            var itineraries = new List<ItineraryViewModel>
            {
                new ItineraryViewModel
                {
                    Title = "Boat Trip",
                    Description = "Explore the beautiful canals of Venice.",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/17/Panorama_of_Canal_Grande_and_Ponte_di_Rialto%2C_Venice_-_September_2017.jpg"
                },
                new ItineraryViewModel
                {
                    Title = "Restaurant Trip",
                    Description = "Visit the iconic restaurants of Venice.",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a1/Venice_Prosecco_and_Cicchetti.jpg"
                },
                new ItineraryViewModel
                {
                    Title = "Pub Trip",
                    Description = "Experience the bustling city life with some wine.",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/42/%22_05_-_ITALY_-_un_bacaro_a_Venezia_Osteria_appetizers_restaurant_in_Venice_wine_enoteca.jpg"
                }
            };

            // Return the view with the list of itineraries
            return View(itineraries);
        }

        // Action method to display a list of pre-made itineraries using HTTP GET
        [HttpGet]
        public IActionResult PreMade()
        {
            // Retrieve the list of pre-made itineraries from a static method or service
            List<Itinerary> itineraries = PreMadeItineraries.GetPreMadeItineraries();
            // Return the view with the list of pre-made itineraries
            return View(itineraries);
        }

        // Action method to handle the selection of pre-made itineraries using HTTP POST
        [HttpPost]
        public async Task<IActionResult> PreMade(int[] selectedItineraries)
        {
            // Get the current user's ID
            string userId = GetCurrentUserId();

            // Loop through each selected itinerary ID
            foreach (int itineraryId in selectedItineraries)
            {
                // Find the selected pre-made itinerary by its ID
                Itinerary preMadeItinerary = PreMadeItineraries
                    .GetPreMadeItineraries()
                    .FirstOrDefault(i => i.Id == itineraryId);

                // If the pre-made itinerary is found
                if (preMadeItinerary != null)
                {
                    // Retrieve the location data associated with the selected pre-made itinerary
                    List<LocationData> selectedLocationDatas = preMadeItinerary.LocationDatas;

                    // Create a new Itinerary object and set its properties
                    Itinerary itinerary = new Itinerary
                    {
                        Name = preMadeItinerary.Name,
                        UserId = userId,
                        Date = DateTime.UtcNow
                    };

                    // Initialize the ItineraryLocationDatas collection if it's null
                    if (itinerary.ItineraryLocationDatas == null)
                    {
                        itinerary.ItineraryLocationDatas = new List<ItineraryLocationData>();
                    }

                    // Loop through each location data in the selected location data list
                    foreach (LocationData locationData in selectedLocationDatas)
                    {
                        // Check if the location data already exists in the database
                        LocationData existingLocationData = context.LocationDatas.FirstOrDefault(
                            ld => ld.Id == locationData.Id
                        );

                        // If the location data exists, add it to the itinerary's location data list
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

                    // Calculate the total cost per person for the selected locations
                    decimal totalCostPerPerson = (decimal)selectedLocationDatas.Sum(ld => ld.PricePerPerson);

                    // Calculate the total cost per itinerary
                    decimal totalCostPerItinerary = totalCostPerPerson * itinerary.NumberOfPeople;

                    // Set the total cost per itinerary
                    itinerary.TotalCostPerItinerary = totalCostPerItinerary;

                    // Add the new itinerary to the database context
                    context.Itineraries.Add(itinerary);
                }
            }

            // Save the changes to the database asynchronously
            await context.SaveChangesAsync();

            // Redirect to the Success action
            return RedirectToAction("Success");
        }

        // Action method to display the itinerary creation form using HTTP GET
        [HttpGet]
        public IActionResult Create()
        {
            // Create a new instance of the CreateItineraryViewModel
            CreateItineraryViewModel viewModel = new CreateItineraryViewModel();

            // Populate AvailableCategories with distinct categories from LocationDatas table
            viewModel.AvailableCategories = context
                .LocationDatas.Select(ld => ld.Category)
                .Distinct()
                .ToList();

            // Populate AvailableLocations with all locations from LocationDatas table
            viewModel.AvailableLocations = context.LocationDatas.ToList();

            // Return the view with the populated view model
            return View(viewModel);
        }

        // Action method to handle the itinerary creation form submission using HTTP POST
        [HttpPost]
        public async Task<IActionResult> Create(CreateItineraryViewModel createItineraryViewModel, int numberOfPets)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Get the current user's ID
                string userId = GetCurrentUserId();

                // Check if "All Categories" is selected
                if (createItineraryViewModel.SelectedCategories != null && createItineraryViewModel.SelectedCategories.Contains("All"))
                {
                    // If "All" is selected, include all available categories
                    createItineraryViewModel.SelectedCategories = await context.LocationDatas
                        .Select(ld => ld.Category)
                        .Distinct()
                        .ToListAsync();
                }

                // If "All Categories" is selected, include all location IDs
                if (createItineraryViewModel.SelectedCategories.Contains("All"))
                {
                    createItineraryViewModel.SelectedLocationIds = await context.LocationDatas
                        .Select(ld => ld.Id)
                        .ToListAsync();
                }

                // Retrieve selected location datas based on user's selection
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
                    NumberOfPeople = createItineraryViewModel.NumberOfPeople,
                    NumberOfPets = numberOfPets
                };

                // Calculate total cost per person for selected locations
                decimal totalCostPerPerson = (decimal)selectedLocationDatas.Sum(ld => ld.PricePerPerson);

                // Calculate total cost per itinerary
                decimal totalCostPerItinerary = totalCostPerPerson * itinerary.NumberOfPeople;

                itinerary.TotalCostPerItinerary = totalCostPerItinerary;

                // Add itinerary to context and save changes
                context.Itineraries.Add(itinerary);
                await context.SaveChangesAsync();

                // Redirect to the Success action
                return RedirectToAction("Success");
            }

            // If ModelState is not valid, re-populate view model and return the view with errors
            createItineraryViewModel.AvailableCategories = await context
                .LocationDatas.Select(ld => ld.Category)
                .Distinct()
                .ToListAsync();
            createItineraryViewModel.AvailableLocations = await context.LocationDatas.ToListAsync();

            return View(createItineraryViewModel);
        }

        // Action method to display a list of itineraries for the current user using HTTP GET
        public async Task<IActionResult> Success()
        {
            // Get the current user's ID
            string userId = GetCurrentUserId();

            // Retrieve itineraries associated with the current user and include related location data
            List<Itinerary> itineraries = await context
                .Itineraries.Where(i => i.UserId == userId)
                .Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .ToListAsync();

            // Return the view with the list of itineraries
            return View(itineraries);
        }

        // Action method to view locations associated with a specific itinerary using HTTP GET
        public async Task<IActionResult> ViewLocations(int itineraryId)
        {
            // Get the current user's ID
            string userId = GetCurrentUserId();

            // Retrieve the itinerary based on user ID and itinerary ID, and include related location data
            Itinerary itinerary = await context
                .Itineraries.Where(i => i.UserId == userId && i.Id == itineraryId)
                .Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .FirstOrDefaultAsync();

            // If the itinerary is not found, return a 404 Not Found result
            if (itinerary == null)
            {
                return NotFound();
            }

            // Return the view with the itinerary details
            return View(itinerary);
        }

        // Action method to display the itinerary deletion form using HTTP GET
        [HttpGet]
        public IActionResult Delete()
        {
            // Get the current user's ID
            string userId = GetCurrentUserId();

            // Retrieve the list of itineraries for the current user
            List<Itinerary> itineraries = context
                .Itineraries.Where(i => i.UserId == userId)
                .ToList();

            // Return the view with the list of itineraries for deletion
            return View("Delete", itineraries);
        }

        // Action method to handle the deletion of itineraries using HTTP POST
        [HttpPost]
        public async Task<IActionResult> Delete(int[] ItineraryIds)
        {
            // Loop through each selected itinerary ID
            foreach (int id in ItineraryIds)
            {
                // Find the itinerary by its ID
                Itinerary? theItinerary = await context.Itineraries.FindAsync(id);
                // If the itinerary is found, remove it from the context
                if (theItinerary != null)
                {
                    context.Itineraries.Remove(theItinerary);
                }
            }
            // Save the changes to the database asynchronously
            await context.SaveChangesAsync();

            // Get the current user's ID
            string userId = GetCurrentUserId();
            // Retrieve the updated list of itineraries for the current user
            List<Itinerary> itineraries = await context
                .Itineraries.Where(i => i.UserId == userId)
                .ToListAsync();

            // Return the view with the updated list of itineraries for deletion
            return View("Delete", itineraries);
        }

        // Action method to display the itinerary edit form using HTTP GET
        [HttpGet]
        public async Task<IActionResult> Edit(int itineraryId)
        {
            // Get the current user's ID
            string userId = GetCurrentUserId();

            // Retrieve the itinerary based on user ID and itinerary ID, and include related location data
            Itinerary itinerary = await context
                .Itineraries.Where(i => i.UserId == userId && i.Id == itineraryId)
                .Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .FirstOrDefaultAsync();

            // If the itinerary is not found, return a 404 Not Found result
            if (itinerary == null)
            {
                return NotFound();
            }

            // Create and populate the EditItineraryViewModel
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

            // Return the view with the populated view model for editing
            return View(viewModel);
        }

        // Action method to handle the itinerary edit form submission using HTTP POST
        [HttpPost]
        public async Task<IActionResult> Edit(EditItineraryViewModel editViewModel)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Get the current user's ID
                string userId = GetCurrentUserId();

                // Retrieve the itinerary to be edited based on user ID and itinerary ID
                Itinerary itinerary = await context
                    .Itineraries.Include(i => i.ItineraryLocationDatas)
                    .FirstOrDefaultAsync(i =>
                        i.Id == editViewModel.ItineraryId && i.UserId == userId
                    );

                // If the itinerary is not found, return a 404 Not Found result
                if (itinerary == null)
                {
                    return NotFound();
                }

                // Update the itinerary's properties
                itinerary.Name = editViewModel.Name;
                itinerary.Date = editViewModel.Date.Date;

                // Clear the existing location data for the itinerary
                itinerary.ItineraryLocationDatas.Clear();

                // Query to filter location data based on selected categories
                IQueryable<LocationData> locationDatasQuery = context.LocationDatas.AsQueryable();

                // Apply category filtering if selected categories are provided
                if (
                    editViewModel.SelectedCategories != null
                    && editViewModel.SelectedCategories.Any()
                )
                {
                    locationDatasQuery = locationDatasQuery.Where(ld =>
                        editViewModel.SelectedCategories.Contains(ld.Category)
                    );
                }

                // Retrieve the selected location data based on user's selection
                List<LocationData> selectedLocationDatas = await locationDatasQuery
                    .Where(ld => editViewModel.SelectedLocationIds.Contains(ld.Id))
                    .ToListAsync();

                // Add the selected location data to the itinerary
                foreach (LocationData locationData in selectedLocationDatas)
                {
                    itinerary.ItineraryLocationDatas.Add(
                        new ItineraryLocationData { LocationDataId = locationData.Id }
                    );
                }

                // Calculate the total cost per person for selected locations
                decimal totalCostPerPerson = (decimal)selectedLocationDatas.Sum(ld => ld.PricePerPerson);

                // Calculate the total cost per itinerary
                decimal totalCostPerItinerary = totalCostPerPerson * itinerary.NumberOfPeople;

                // Set the total cost per itinerary
                itinerary.TotalCostPerItinerary = totalCostPerItinerary;

                // Save changes to the database
                await context.SaveChangesAsync();

                // Redirect to the ViewLocations action with the updated itinerary ID
                return RedirectToAction("ViewLocations", new { itineraryId = itinerary.Id });
            }

            // If ModelState is not valid, re-populate view model and return the view with errors
            editViewModel.AvailableCategories = context
                .LocationDatas.Select(ld => ld.Category)
                .Distinct()
                .ToList();
            editViewModel.AvailableLocations = context.LocationDatas.ToList();

            return View(editViewModel);
        }

        // Action method to calculate and display the total cost for all locations and people
        [HttpPost]
        public IActionResult CalculateTotalCost(int itineraryId, int numberOfPeople)
        {
            // Get the current user's ID
            string userId = GetCurrentUserId();

            // Retrieve the itinerary based on user ID and itinerary ID, and include related location data
            var itinerary = context
                .Itineraries.Include(i => i.ItineraryLocationDatas)
                .ThenInclude(il => il.LocationData)
                .FirstOrDefault(i => i.UserId == userId && i.Id == itineraryId);

            // If the itinerary is not found, return a 404 Not Found result
            if (itinerary == null)
            {
                return NotFound();
            }

            // Calculate the total cost for all locations
            decimal totalCostForAllLocations = CalculateTotalCostForLocations(itinerary);

            // Calculate the total cost for all people
            decimal totalCostForAllPeople = totalCostForAllLocations * numberOfPeople;

            // Update the itinerary with the calculated costs
            itinerary.TotalCostForAllLocations = totalCostForAllLocations;
            itinerary.TotalCostForAllPeople = totalCostForAllPeople;
            itinerary.NumberOfPeople = numberOfPeople;

            // Save changes to the database
            context.SaveChanges();

            // Return the view with the updated itinerary details
            return View("ViewLocations", itinerary);
        }

        // Private method to calculate the total cost for all locations in an itinerary
        private decimal CalculateTotalCostForLocations(Itinerary itinerary)
        {
            // Sum up the price per person for all location data in the itinerary
            return (decimal)
                itinerary.ItineraryLocationDatas.Sum(il => (double)il.LocationData.PricePerPerson);
        }
    }
}
