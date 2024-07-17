using System;
using System.Collections.Generic;
using Trips.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc;

public class CreateItineraryViewModel
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Please select at least one location")]
    public List<int> SelectedLocationIds { get; set; }

    public List<LocationData> AvailableLocations { get; set; }

    public List<string> AvailableCategories { get; set; }

    public List<string> SelectedCategories { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
    public int NumberOfPeople { get; set; }

    public decimal TotalCost { get; set; }

     public int NumberOfPets { get; set; }  
    public CreateItineraryViewModel()
    {
        AvailableLocations = new List<LocationData>();
        SelectedLocationIds = new List<int>();
        AvailableCategories = new List<string>();
        SelectedCategories = new List<string>();
    }
}
