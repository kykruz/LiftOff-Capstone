using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using Trips.Models;

namespace Exchange.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRatesApiService : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "osBudYFRdkNVt6966P10CnRGMpEDIZK3";

        private readonly string _baseUrl = "https://api.apilayer.com/exchangerates_data";

       
        public ExchangeRatesApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
        }

        [HttpGet("convert")]
        public async Task<ConvertResponse> ConvertAsync(
            string fromCurrency,
            string toCurrency,
            double amount
        )
        {
            string apiUrl =
                $"{_baseUrl}/convert?to={toCurrency}&from={fromCurrency}&amount={amount}";
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("apikey", _apiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ConvertResponse>(json);
            }
            else
            {
                throw new HttpRequestException(
                    $"Error fetching data from API: {response.ReasonPhrase}"
                );
            }
        }
    }

    public class ExchangeRateResponse
    {
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
