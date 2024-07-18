using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Exchange.Services
{
    public class ExchangeRatesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "osBudYFRdkNVt6966P10CnRGMpEDIZK3";
        
        private readonly string _baseUrl = "https://api.apilayer.com/exchangerates_data";
        

        public ExchangeRatesApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConvertResponse> ConvertAsync(ConvertRequest request)
        {
            var url = $"{_baseUrl}/convert?from={request.FromCurrency}&to={request.ToCurrency}&amount={request.Amount}&apiKey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ConvertResponse>(response);
        }
    }

    public class ConvertRequest
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public double Amount { get; set; }

        public ConvertRequest(string fromCurrency, string toCurrency, double amount)
        {
            FromCurrency = fromCurrency;
            ToCurrency = toCurrency;
            Amount = amount;
        }
    }

    public class ConvertResponse
    {
        public double Result { get; set; }
    }
}