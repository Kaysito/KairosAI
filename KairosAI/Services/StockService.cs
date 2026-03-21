using System.Net;
using System.Net.Http.Json;
using KairosAI.Models.Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KairosAI.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<StockService> _logger;
        private readonly string _apiKey;
        private bool _hasValidKey;

        private const string CACHE_QUOTES = "stock_quotes_{0}";
        private const string CACHE_NEWS = "stock_news_{0}_{1}";

        public StockService(
            HttpClient http,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<StockService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
            _apiKey = config["ApiKeys:StockData"] ?? "";

            // Validación de API key
            _hasValidKey = !string.IsNullOrWhiteSpace(_apiKey)
                           && _apiKey != "654321"
                           && _apiKey != "tu-key-aqui"
                           && _apiKey.Length > 10;

            if (!_hasValidKey)
            {
                _logger.LogWarning(
                    "⚠️ StockData API key no configurada o es de ejemplo. " +
                    "Las noticias y cotizaciones de bolsa no estarán disponibles. " +
                    "Regístrate gratis en https://www.stockdata.org");
            }

            _http.BaseAddress = new Uri("https://api.stockdata.org/v1/");

            if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _http.DefaultRequestHeaders.Add("User-Agent", "KairosAI/1.0");
            }
        }

        public async Task<List<StockQuote>> GetQuotesAsync(string[] tickers)
        {
            if (!_hasValidKey)
            {
                _logger.LogDebug("StockData: Saltando GetQuotesAsync porque no hay API key válida");
                return new List<StockQuote>();
            }

            string tickersJoined = string.Join(",", tickers);
            string cacheKey = string.Format(CACHE_QUOTES, tickersJoined);

            if (_cache.TryGetValue(cacheKey, out List<StockQuote>? cached) && cached != null)
                return cached;

            try
            {
                string url = $"data/quote?symbols={tickersJoined}&api_token={_apiKey}";

                _logger.LogDebug("StockData Quotes Request: {Url}",
                    url.Replace(_apiKey, "***HIDDEN***"));

                var response = await _http.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning(
                        "StockData 401: API key inválida o expirada. " +
                        "Verifica tu key en https://www.stockdata.org/dashboard");
                    _hasValidKey = false;
                    return new List<StockQuote>();
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("StockData 429: Límite de requests excedido al obtener cotizaciones");
                    return new List<StockQuote>();
                }

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<StockDataResponse>();

                if (data?.Data != null)
                {
                    _cache.Set(cacheKey, data.Data, TimeSpan.FromMinutes(5));
                    return data.Data;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error HTTP al obtener cotizaciones de StockData");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error general al obtener cotizaciones de StockData");
            }

            return new List<StockQuote>();
        }

        public async Task<List<StockDataNewsItem>> GetNewsAsync(
            string? search = null,
            string? tickers = null,
            int limit = 20,
            string language = "es")
        {
            if (!_hasValidKey)
            {
                _logger.LogDebug("StockData: Saltando GetNewsAsync porque no hay API key válida");
                return new List<StockDataNewsItem>();
            }

            string cacheKey = string.Format(CACHE_NEWS, search ?? "all", tickers ?? "none");

            if (_cache.TryGetValue(cacheKey, out List<StockDataNewsItem>? cached) && cached != null)
                return cached;

            try
            {
                var queryParts = new List<string>
                {
                    $"api_token={_apiKey}",
                    $"language={language}",
                    $"limit={limit}",
                    "sort=published_at",
                    "sort_order=desc",
                    "filter_entities=true",
                    "must_have_entities=true"
                };

                if (!string.IsNullOrEmpty(search))
                    queryParts.Add($"search={Uri.EscapeDataString(search)}");

                if (!string.IsNullOrEmpty(tickers))
                    queryParts.Add($"symbols={tickers}");

                string url = $"news/all?{string.Join("&", queryParts)}";

                _logger.LogDebug("StockData News Request: {Url}",
                    url.Replace(_apiKey, "***HIDDEN***"));

                var response = await _http.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning(
                        "StockData 401: API key inválida o expirada. " +
                        "Verifica tu key en https://www.stockdata.org/dashboard");
                    _hasValidKey = false;
                    return new List<StockDataNewsItem>();
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("StockData 429: Rate limit excedido al obtener noticias");
                    return new List<StockDataNewsItem>();
                }

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<StockDataNewsResponse>();

                if (data?.Data != null)
                {
                    _cache.Set(cacheKey, data.Data, TimeSpan.FromMinutes(5));
                    return data.Data;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error HTTP al obtener noticias de StockData");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error general al obtener noticias de StockData");
            }

            return new List<StockDataNewsItem>();
        }
    }
}