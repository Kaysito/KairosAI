using System.Net.Http.Json;
using System.Text.Json;
using KairosAI.Models.Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KairosAI.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CryptoService> _logger;
        private readonly string _apiKey;

        // Cache keys
        private const string CACHE_MARKET = "crypto_market_{0}_{1}";
        private const string CACHE_TRENDING = "crypto_trending";
        private const string CACHE_PRICES = "crypto_prices_{0}";

        public CryptoService(
            HttpClient http,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<CryptoService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
            _apiKey = config["ApiKeys:CoinGecko"] ?? "";

            _http.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
            _http.DefaultRequestHeaders.Add("accept", "application/json");

            // CoinGecko Demo/Pro key header
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _http.DefaultRequestHeaders.Add("x-cg-demo-api-key", _apiKey);
            }
        }

        public async Task<List<CoinGeckoMarketItem>> GetMarketDataAsync(
            string vsCurrency = "usd",
            int perPage = 10,
            string[]? ids = null)
        {
            string idsParam = ids != null ? string.Join(",", ids) : "";
            string cacheKey = string.Format(CACHE_MARKET, vsCurrency, idsParam + perPage);

            if (_cache.TryGetValue(cacheKey, out List<CoinGeckoMarketItem>? cached) && cached != null)
                return cached;

            try
            {
                string url = $"coins/markets?vs_currency={vsCurrency}&order=market_cap_desc&per_page={perPage}&page=1&sparkline=true";

                if (!string.IsNullOrEmpty(idsParam))
                    url += $"&ids={idsParam}";

                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<List<CoinGeckoMarketItem>>();

                if (data != null)
                {
                    // Cache por 2 minutos (CoinGecko free tier: ~30 calls/min)
                    _cache.Set(cacheKey, data, TimeSpan.FromMinutes(2));
                    return data;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error al obtener datos de CoinGecko markets");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error al deserializar respuesta de CoinGecko");
            }

            return new List<CoinGeckoMarketItem>();
        }

        public async Task<CoinGeckoTrendingResponse?> GetTrendingAsync()
        {
            if (_cache.TryGetValue(CACHE_TRENDING, out CoinGeckoTrendingResponse? cached))
                return cached;

            try
            {
                var response = await _http.GetAsync("search/trending");
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<CoinGeckoTrendingResponse>();

                if (data != null)
                {
                    _cache.Set(CACHE_TRENDING, data, TimeSpan.FromMinutes(5));
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener trending de CoinGecko");
                return null;
            }
        }

        public async Task<Dictionary<string, decimal>> GetSimplePricesAsync(
            string[] ids,
            string vsCurrency = "usd")
        {
            string idsJoined = string.Join(",", ids);
            string cacheKey = string.Format(CACHE_PRICES, idsJoined);

            if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? cached) && cached != null)
                return cached;

            try
            {
                string url = $"simple/price?ids={idsJoined}&vs_currencies={vsCurrency}&include_24hr_change=true";
                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>();

                var result = new Dictionary<string, decimal>();
                if (json != null)
                {
                    foreach (var kvp in json)
                    {
                        if (kvp.Value.ContainsKey(vsCurrency))
                            result[kvp.Key] = kvp.Value[vsCurrency];
                    }
                }

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener precios simples de CoinGecko");
                return new Dictionary<string, decimal>();
            }
        }
    }
}