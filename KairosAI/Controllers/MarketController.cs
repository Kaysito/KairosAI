using KairosAI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // Necesario para leer configuraciones
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

        public MarketController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
            // 🛡️ Extraemos la llave del appsettings.json
            _coinGeckoKey = _config["ApiKeys:CoinGecko"];
        }

        public async Task<IActionResult> Index(string searchTerm, string filter = "Todos")
        {
            var model = new MarketIndexViewModel
            {
                SearchTerm = searchTerm,
                Filter = filter
            };

            // Obtenemos los activos (Criptos + Bolsa Simulada)
            var assets = await GetFullMarketData();

            // 🔍 Lógica de Filtro por Tipo (Cripto o Accion)
            if (filter != "Todos")
            {
                assets = assets.Where(a => a.Type == filter).ToList();
            }

            // 🔍 Lógica de Búsqueda Dinámica
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

            // 1. OBTENER CRIPTOMONEDAS REALES
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=50&page=1&sparkline=false&price_change_percentage=24h");

                request.Headers.Add("x-cg-demo-api-key", _coinGeckoKey);
                request.Headers.Add("User-Agent", "KairosAI-MarketView/2.0");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    int rank = 1;
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        allAssets.Add(new MarketAsset
                        {
                            Symbol = element.GetProperty("symbol").GetString().ToUpper(),
                            Name = element.GetProperty("name").GetString(),
                            Price = element.GetProperty("current_price").GetDecimal(),
                            Change24h = element.GetProperty("price_change_percentage_24h").GetDouble(),
                            ImageUrl = element.GetProperty("image").GetString(),
                            Type = "Cripto",
                            // Lógica de riesgo: Top 10 es bajo, resto aumenta.
                            RiskLevel = rank <= 10 ? "Bajo" : (rank <= 30 ? "Medio" : "Alto"),
                            IsTrending = rank <= 3 // Solo las top 3 llevan fuego 🔥
                        });
                        rank++;
                    }
                }
            }
            catch { /* Fallback si falla la API de Cripto */ }

            // 2. OBTENER BOLSA DE VALORES (Simulado por ahora para dar vida a la sección)
            // Cuando consigas una API de Stocks (como AlphaVantage), aquí harías la llamada similar a la de arriba.
            allAssets.AddRange(new List<MarketAsset>
            {
                new() { Symbol = "NVDA", Name = "NVIDIA Corp", Price = 135.20m, Change24h = 2.45, Type = "Accion", RiskLevel = "Medio", IsTrending = true, ImageUrl = "" },
                new() { Symbol = "AAPL", Name = "Apple Inc", Price = 192.50m, Change24h = -0.32, Type = "Accion", RiskLevel = "Bajo", IsTrending = false, ImageUrl = "" },
                new() { Symbol = "TSLA", Name = "Tesla Motors", Price = 250.10m, Change24h = 5.12, Type = "Accion", RiskLevel = "Alto", IsTrending = true, ImageUrl = "" },
                new() { Symbol = "MSFT", Name = "Microsoft", Price = 410.15m, Change24h = 0.85, Type = "Accion", RiskLevel = "Bajo", IsTrending = false, ImageUrl = "" },
                new() { Symbol = "AMZN", Name = "Amazon.com", Price = 175.40m, Change24h = 1.10, Type = "Accion", RiskLevel = "Bajo", IsTrending = false, ImageUrl = "" }
            });

            return allAssets.OrderByDescending(a => a.IsTrending).ToList();
        }

        // 3. VISTA DETALLADA: Preparada para escalar y perfecta para la demo actual
        public async Task<IActionResult> Details(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return RedirectToAction("Index");
            }

            symbol = symbol.ToUpper();

            // 🚀 ESTADO ACTUAL: MOCK DATA PARA LA DEMOSTRACIÓN (Para no depender de llamadas extra a la API hoy)
            // 🛠️ FUTURO: Aquí harías un 'await _httpClient.SendAsync(...)' al endpoint '/coins/{id}/market_chart' de CoinGecko

            var model = new AssetDetailViewModel
            {
                Symbol = symbol,

                // Asignación dinámica básica para darle realismo a cualquier moneda que elijan en la tabla
                Name = symbol == "BTC" ? "Bitcoin" : (symbol == "ETH" ? "Ethereum" : symbol + " Asset"),
                Price = symbol == "BTC" ? 64320.50m : (symbol == "ETH" ? 3450.75m : 150.25m),
                Change24h = symbol == "BTC" ? 2.45 : (symbol == "ETH" ? -1.12 : 5.67),
                RiskLevel = symbol == "BTC" || symbol == "AAPL" ? "Bajo" : "Medio",
                IsTrending = symbol == "BTC" || symbol == "NVDA",
                Type = symbol == "NVDA" || symbol == "AAPL" ? "Accion" : "Cripto",

                // Si es Cripto, intentamos jalar un logo genérico. Si es Acción, la vista usará el fallback.
                ImageUrl = symbol.Length <= 4 && symbol != "NVDA" && symbol != "AAPL" ? $"https://logo.synthfinance.com/ticker/{symbol}" : "",

                // Fundamentales estáticos
                MarketCap = symbol == "BTC" ? "$1.2T" : (symbol == "ETH" ? "$400B" : "$10B"),
                Volume24h = symbol == "BTC" ? "$35.2B" : "$12.5B",
                High24h = symbol == "BTC" ? 65100m : 3600m,
                Low24h = symbol == "BTC" ? 62000m : 3300m,

                // Puntos de la gráfica (Últimas horas simuladas)
                ChartLabels = new List<string> { "10:00", "11:00", "12:00", "13:00", "14:00", "15:00" }
            };

            // Simular curva alcista o bajista según el Change24h
            if (model.Change24h >= 0)
            {
                model.ChartData = new List<decimal> { model.Price * 0.95m, model.Price * 0.96m, model.Price * 0.94m, model.Price * 0.98m, model.Price * 0.99m, model.Price };
            }
            else
            {
                model.ChartData = new List<decimal> { model.Price * 1.05m, model.Price * 1.04m, model.Price * 1.06m, model.Price * 1.02m, model.Price * 1.01m, model.Price };
            }

            // El veredicto simulado de KairósAI
            model.KairosSentiment = model.Change24h >= 0 ? "Alcista" : "Bajista";
            model.KairosAnalysis = model.Change24h >= 0
                ? "El modelo de KairósAI detecta una fuerte presión de compra institucional. La estructura técnica se mantiene firme por encima de la media móvil exponencial de 50 periodos.<br/><br/><b>Recomendación:</b> Mantener posiciones con stop-loss ajustado."
                : "El radar algorítmico indica debilidad en el flujo de órdenes a corto plazo. Se observa una divergencia bajista en los marcos temporales menores.<br/><br/><b>Recomendación:</b> Esperar confirmación de soporte antes de abrir nuevas posiciones.";

            return View(model);
        }
    }
}