namespace Trips.Models;

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
    }

    public LocationData() { }

    public static implicit operator List<object>(LocationData v)
    {
        throw new NotImplementedException();
    }

    internal LocationData Where(Func<object, object> value)
    {
        throw new NotImplementedException();
    }
}
