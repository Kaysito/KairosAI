using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KairosAI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _coinGeckoKey;

        public DashboardController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;

            // Leemos la llave de forma segura
            _coinGeckoKey = _config["ApiKeys:CoinGecko"] ?? string.Empty;
        }

        public async Task<IActionResult> Index()
        {
            // 1. RECUPERAR DATOS DEL TEST (Parseo seguro para evitar Crash/Loading eterno)
            int aggression = 4; // Valor por defecto (Moderado)
            if (TempData["UserAggressionAxis"] != null && int.TryParse(TempData["UserAggressionAxis"].ToString(), out int parsedAgg))
            {
                aggression = parsedAgg;
            }

            int knowledge = 3; // Valor por defecto (Intermedio)
            if (TempData["UserKnowledgeAxis"] != null && int.TryParse(TempData["UserKnowledgeAxis"].ToString(), out int parsedKnow))
            {
                knowledge = parsedKnow;
            }

            // Mantenemos los datos en TempData por si el usuario recarga la página
            TempData.Keep("UserAggressionAxis");
            TempData.Keep("UserKnowledgeAxis");

            // 2. DETERMINAR PERFIL PSICOLÓGICO Y FRICCIÓN (Lógica de Negocio Kairós)
            string riskProfile = "Equilibrado";
            int frictionLevel = 2; // Moderado por defecto

            if (aggression <= 3 && knowledge <= 3)
            {
                riskProfile = "Cauteloso";
                frictionLevel = 3;
            }
            else if (aggression >= 5 && knowledge <= 3)
            {
                riskProfile = "Principiante Agresivo";
                frictionLevel = 3;
            }
            else if (aggression >= 5 && knowledge >= 5)
            {
                riskProfile = "Alto Vuelo";
                frictionLevel = 1;
            }
            else if (knowledge >= 4)
            {
                riskProfile = "Explorador";
                frictionLevel = 2;
            }

            // 3. OBTENER DATOS REALES DE MERCADO
            var realAssets = await GetRealMarketData();

            // 4. GENERAR EL MODELO FINAL
            var model = new DashboardViewModel
            {
                TotalBalance = 245300.50m,
                MonthlyReturn = 12.5,

                AggressionAxis = aggression,
                KnowledgeAxis = knowledge,
                RiskProfile = riskProfile,
                FrictionLevel = frictionLevel,
                RiskLabel = riskProfile,

                PerformanceData = new List<ChartDataPoint>
                {
                    new ChartDataPoint { Date = "Lun", Value = 240000 },
                    new ChartDataPoint { Date = "Mar", Value = 242000 },
                    new ChartDataPoint { Date = "Mie", Value = 238000 },
                    new ChartDataPoint { Date = "Jue", Value = 245000 },
                    new ChartDataPoint { Date = "Vie", Value = 248000 },
                    new ChartDataPoint { Date = "Sab", Value = 245300 },
                    new ChartDataPoint { Date = "Dom", Value = 245300 }
                },

                TopAssets = realAssets.Count > 0 ? realAssets : new List<AssetSummary>(),

                RecentNews = new List<NewsItem>
                {
                    new NewsItem { Title = "KairósAI detecta fuerte entrada institucional en BTC", Source = "Bloomberg", TimeAgo = "Hace 2h", Sentiment = "Positivo" },
                    new NewsItem { Title = "Nuevos ETFs de Ethereum inician cotización", Source = "CoinDesk", TimeAgo = "Hace 4h", Sentiment = "Positivo" },
                    new NewsItem { Title = "Reguladores de la UE anuncian nuevo marco MiCA", Source = "Reuters", TimeAgo = "Hace 5h", Sentiment = "Neutro" },
                    new NewsItem { Title = "Volatilidad en mercados asiáticos afecta criptomonedas", Source = "WSJ", TimeAgo = "Hace 8h", Sentiment = "Negativo" }
                }
            };

            return View(model);
        }

        // ── CORRECCIÓN DEL PARSEO JSON PARA EVITAR ERRORES DE SINTAXIS ──
        private async Task<List<AssetSummary>> GetRealMarketData()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=10&page=1&sparkline=false&price_change_percentage=24h");

                if (!string.IsNullOrEmpty(_coinGeckoKey))
                {
                    request.Headers.Add("x-cg-demo-api-key", _coinGeckoKey);
                }

                request.Headers.Add("User-Agent", "KairosAI-Dashboard/2.5");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return new List<AssetSummary>();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return new List<AssetSummary>();

                var assets = new List<AssetSummary>();

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    // 1. Leemos las variables FUERA del inicializador del objeto
                    string symbol = element.TryGetProperty("symbol", out var symProp) && symProp.ValueKind == JsonValueKind.String
                        ? (symProp.GetString()?.ToUpper() ?? "") : "";

                    string name = element.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                        ? (nameProp.GetString() ?? "") : "";

                    decimal price = element.TryGetProperty("current_price", out var priceProp) && priceProp.ValueKind == JsonValueKind.Number
                        ? priceProp.GetDecimal() : 0m;

                    double change = element.TryGetProperty("price_change_percentage_24h", out var changeProp) && changeProp.ValueKind == JsonValueKind.Number
                        ? changeProp.GetDouble() : 0;

                    string image = element.TryGetProperty("image", out var imgProp) && imgProp.ValueKind == JsonValueKind.String
                        ? (imgProp.GetString() ?? "") : "";

                    string risk = element.TryGetProperty("market_cap_rank", out var rankProp) && rankProp.ValueKind == JsonValueKind.Number && rankProp.GetInt32() <= 3
                        ? "Bajo" : "Medio";

                    // 2. Asignamos de forma limpia y directa
                    assets.Add(new AssetSummary
                    {
                        Symbol = symbol,
                        Name = name,
                        Price = price,
                        Change24h = change,
                        ImageUrl = image,
                        RiskLevel = risk
                    });
                }
                return assets;
            }
            catch
            {
                return new List<AssetSummary>();
            }
        }
    }
}