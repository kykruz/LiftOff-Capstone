using System;
using System.Linq;

namespace Trips.Models
{
    public class LocationData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Category { get; set; }
        public double PricePerPerson { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public bool IsPetFriendly { get; set; }

        public List<ItineraryLocationData> ItineraryLocationDatas { get; set; } = new List<ItineraryLocationData>();

        public LocationData(
            int id,
            string name,
            string address,
            string category,
            double pricePerPerson,
            string description,
            string phone
        )
        {
            Id = id;
            Name = name;
            Address = address;
            Category = category;
            PricePerPerson = pricePerPerson;
            Description = description;
            Phone = phone;
            IsPetFriendly = LocationUtilities.IsPetFriendly(name);
        }

        public LocationData() { }
    }

    public static class LocationUtilities
    {
        public static bool IsPetFriendly(string locationName)
        {
            string[] petFriendlyKeywords = { "park", "parco", "pet" };

            string lowercaseLocationName = locationName.ToLower();

            return lowercaseLocationName.Contains("dog") ||
                   petFriendlyKeywords.Any(keyword => lowercaseLocationName.Contains(keyword));
        }
    }
}

