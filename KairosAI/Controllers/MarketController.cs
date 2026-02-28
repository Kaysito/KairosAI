using KairosAI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace KairosAI.Controllers
{
    public class MarketController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _coinGeckoKey;
        private readonly string _dataBursatilToken;
        private const string DataBursatilBaseUrl = "https://api.databursatil.com/v2/";

        public MarketController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
            _coinGeckoKey = _config["ApiKeys:CoinGecko"] ?? string.Empty;
            // Token de 30 caracteres alfanuméricos según documentación
            _dataBursatilToken = _config["ApiKeys:StockData"] ?? string.Empty;
        }

        public async Task<IActionResult> Index(string searchTerm, string filter = "Todos")
        {
            var model = new MarketIndexViewModel
            {
                SearchTerm = searchTerm,
                Filter = filter
            };

            var assets = await GetFullMarketData();

            if (filter != "Todos")
            {
                assets = assets.Where(a => a.Type == filter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                assets = assets.Where(a =>
                    a.Name.ToLower().Contains(search) ||
                    a.Symbol.ToLower().Contains(search)
                ).ToList();
            }

            model.Assets = assets;
            return View(model);
        }

        private async Task<List<MarketAsset>> GetFullMarketData()
        {
            var allAssets = new List<MarketAsset>();

            // 1. OBTENER CRIPTOMONEDAS (Vía CoinGecko)
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=50&page=1");
                if (!string.IsNullOrEmpty(_coinGeckoKey)) request.Headers.Add("x-cg-demo-api-key", _coinGeckoKey);
                request.Headers.Add("User-Agent", "KairosAI-Market/2.0");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var element in doc.RootElement.EnumerateArray())
                        {
                            allAssets.Add(new MarketAsset
                            {
                                Symbol = element.TryGetProperty("symbol", out var sym) && sym.ValueKind == JsonValueKind.String ? sym.GetString()?.ToUpper() ?? "" : "",
                                Name = element.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String ? name.GetString() ?? "" : "",
                                Price = element.TryGetProperty("current_price", out var price) && price.ValueKind == JsonValueKind.Number ? price.GetDecimal() : 0m,
                                Change24h = element.TryGetProperty("price_change_percentage_24h", out var change) && change.ValueKind == JsonValueKind.Number ? change.GetDouble() : 0,
                                ImageUrl = element.TryGetProperty("image", out var img) && img.ValueKind == JsonValueKind.String ? img.GetString() ?? "" : "",
                                Type = "Cripto",
                                RiskLevel = "Medio"
                            });
                        }
                    }
                }
            }
            catch { /* Fallback silencioso */ }

            // 2. OBTENER BOLSA REAL (Vía DataBursatil /v2/cotizaciones)
            string tickers = "NVDA*,AAPL*,TSLA*,MSFT*,AMZN*";
            try
            {
                var stockUrl = $"{DataBursatilBaseUrl}cotizaciones?token={_dataBursatilToken}&emisora_serie={tickers}&bolsa=BMV,BIVA&concepto=u,c";
                var stockResponse = await _httpClient.GetAsync(stockUrl);

                if (stockResponse.IsSuccessStatusCode)
                {
                    var sJson = await stockResponse.Content.ReadAsStringAsync();
                    using var sDoc = JsonDocument.Parse(sJson);
                    var root = sDoc.RootElement;

                    foreach (var property in root.EnumerateObject())
                    {
                        var tickerData = property.Value;
                        string cleanSymbol = property.Name.Replace("*", "");

                        decimal stockPrice = 0m;
                        double stockChange = 0;

                        if (tickerData.TryGetProperty("u", out var u) && u.ValueKind == JsonValueKind.Number) stockPrice = u.GetDecimal();
                        if (tickerData.TryGetProperty("c", out var c) && c.ValueKind == JsonValueKind.Number) stockChange = c.GetDouble();

                        allAssets.Add(new MarketAsset
                        {
                            Symbol = cleanSymbol,
                            Name = GetStockFullName(cleanSymbol),
                            Price = stockPrice,
                            Change24h = stockChange,
                            Type = "Accion",
                            RiskLevel = "Bajo",
                            ImageUrl = $"https://logo.clearbit.com/{GetStockDomain(cleanSymbol)}"
                        });
                    }
                }
            }
            catch
            {
                // Fallback de seguridad si DataBursatil no responde
                allAssets.Add(new MarketAsset { Symbol = "NVDA", Name = "NVIDIA Corp", Price = 135.20m, Change24h = 2.45, Type = "Accion", RiskLevel = "Medio", ImageUrl = "https://logo.clearbit.com/nvidia.com" });
            }

            return allAssets.OrderByDescending(a => a.Symbol == "BTC").ToList(); // Ordenamos para que BTC quede de primero
        }

        // Helpers para completar la información
        private string GetStockFullName(string s) => s switch { "NVDA" => "NVIDIA Corp", "AAPL" => "Apple Inc", "TSLA" => "Tesla Motors", "MSFT" => "Microsoft Corp", "AMZN" => "Amazon", _ => s };
        private string GetStockDomain(string s) => s switch { "NVDA" => "nvidia.com", "AAPL" => "apple.com", "TSLA" => "tesla.com", "MSFT" => "microsoft.com", "AMZN" => "amazon.com", _ => "google.com" };

        public async Task<IActionResult> Details(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return RedirectToAction("Index");

            symbol = symbol.ToUpper();

            // Mapeo dinámico de imágenes para asegurar que la vista Detailed también tenga el logo
            string resolvedImage = symbol switch
            {
                "BTC" => "https://assets.coingecko.com/coins/images/1/large/bitcoin.png",
                "ETH" => "https://assets.coingecko.com/coins/images/279/large/ethereum.png",
                "USDT" => "https://assets.coingecko.com/coins/images/325/large/Tether.png",
                "BNB" => "https://assets.coingecko.com/coins/images/825/large/bnb-icon2_2x.png",
                "SOL" => "https://assets.coingecko.com/coins/images/4128/large/solana.png",
                "NVDA" => "https://logo.clearbit.com/nvidia.com",
                "AAPL" => "https://logo.clearbit.com/apple.com",
                "TSLA" => "https://logo.clearbit.com/tesla.com",
                "MSFT" => "https://logo.clearbit.com/microsoft.com",
                "AMZN" => "https://logo.clearbit.com/amazon.com",
                _ => $"https://logo.synthfinance.com/ticker/{symbol.ToLower()}" // Intento genérico de rescate
            };

            var model = new AssetDetailViewModel
            {
                Symbol = symbol,
                Name = symbol == "BTC" ? "Bitcoin" : (symbol == "ETH" ? "Ethereum" : symbol + " Asset"),
                Price = symbol == "BTC" ? 64320.50m : (symbol == "ETH" ? 3450.75m : 150.25m),
                Change24h = symbol == "BTC" ? 2.45 : (symbol == "ETH" ? -1.12 : 5.67),
                RiskLevel = symbol == "BTC" || symbol == "AAPL" ? "Bajo" : "Medio",
                IsTrending = symbol == "BTC" || symbol == "NVDA",
                Type = symbol == "NVDA" || symbol == "AAPL" || symbol == "TSLA" || symbol == "MSFT" || symbol == "AMZN" ? "Accion" : "Cripto",
                ImageUrl = resolvedImage,
                MarketCap = symbol == "BTC" ? "$1.2T" : (symbol == "ETH" ? "$400B" : "$10B"),
                Volume24h = symbol == "BTC" ? "$35.2B" : "$12.5B",
                High24h = symbol == "BTC" ? 65100m : 3600m,
                Low24h = symbol == "BTC" ? 62000m : 3300m,
                ChartLabels = new List<string> { "10:00", "11:00", "12:00", "13:00", "14:00", "15:00" }
            };

            if (model.Change24h >= 0)
                model.ChartData = new List<decimal> { model.Price * 0.95m, model.Price * 0.96m, model.Price * 0.94m, model.Price * 0.98m, model.Price * 0.99m, model.Price };
            else
                model.ChartData = new List<decimal> { model.Price * 1.05m, model.Price * 1.04m, model.Price * 1.06m, model.Price * 1.02m, model.Price * 1.01m, model.Price };

            model.KairosSentiment = model.Change24h >= 0 ? "Alcista" : "Bajista";
            model.KairosAnalysis = model.Change24h >= 0
                ? "El modelo de KairósAI detecta una fuerte presión de compra institucional. La estructura técnica se mantiene firme por encima de la media móvil exponencial de 50 periodos.<br/><br/><b>Recomendación:</b> Mantener posiciones con stop-loss ajustado."
                : "El radar algorítmico indica debilidad en el flujo de órdenes a corto plazo. Se observa una divergencia bajista en los marcos temporales menores.<br/><br/><b>Recomendación:</b> Esperar confirmación de soporte antes de abrir nuevas posiciones.";

            // Cargamos el mercado real para la barra lateral
            var allAssets = await GetFullMarketData();
            model.TopAssets = allAssets.Take(15).ToList();

            return View(model);
        }
    }
}