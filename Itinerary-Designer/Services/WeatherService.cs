using System;
using System.Net.Http;
using System.Threading.Tasks;

public class WeatherService
{
    private readonly HttpClient _client;

    public WeatherService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://www.meteosource.com/api/v1/free/");
    }

    public async Task<string> GetWeatherAsync(string placeId, string apiKey)
    {
        string endpoint = $"point?place_id={placeId}&sections=all&timezone=UTC&language=en&units=metric&key={apiKey}";

        HttpResponseMessage response = await _client.GetAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            // Handle error
            return null;
        }
    }
}
