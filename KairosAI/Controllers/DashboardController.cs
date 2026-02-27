using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // Necesario para leer el appsettings.json

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

            // 🛡️ Leemos la llave de forma segura
            _coinGeckoKey = _config["ApiKeys:CoinGecko"];
        }

        public async Task<IActionResult> Index()
        {
            // 1. RECUPERAR DATOS DEL TEST
            int userRiskScore = 50;
            if (TempData["UserRiskScore"] != null)
            {
                if (int.TryParse(TempData["UserRiskScore"]?.ToString(), out int score))
                {
                    userRiskScore = score;
                }
            }

            // 2. CALCULAR ETIQUETA DE RIESGO
            string riskLabel = "Moderado";
            if (userRiskScore <= 30) riskLabel = "Conservador";
            else if (userRiskScore > 60) riskLabel = "Agresivo";

            // 3. OBTENER DATOS REALES DE MERCADO
            var realAssets = await GetRealMarketData();

            // 4. GENERAR EL MODELO FINAL
            var model = new DashboardViewModel
            {
                TotalBalance = 245300.50m,
                MonthlyReturn = 12.5,
                RiskScore = userRiskScore,
                RiskLabel = riskLabel,

                PerformanceData = new List<ChartDataPoint>
                {
                    new() { Date = "Lun", Value = 240000 },
                    new() { Date = "Mar", Value = 242000 },
                    new() { Date = "Mie", Value = 238000 },
                    new() { Date = "Jue", Value = 245000 },
                    new() { Date = "Vie", Value = 248000 },
                    new() { Date = "Sab", Value = 245300 },
                    new() { Date = "Dom", Value = 245300 }
                },

                TopAssets = realAssets.Any() ? realAssets : new List<AssetSummary>(),

                RecentNews = new List<NewsItem>
                {
                    new() { Title = "KairósAI detecta fuerte entrada institucional en BTC", Source = "Bloomberg", TimeAgo = "Hace 2h", Sentiment = "Positivo" },
                    new() { Title = "Nuevos ETFs de Ethereum inician cotización", Source = "CoinDesk", TimeAgo = "Hace 4h", Sentiment = "Positivo" },
                    new() { Title = "Reguladores de la UE anuncian nuevo marco MiCA", Source = "Reuters", TimeAgo = "Hace 5h", Sentiment = "Neutro" },
                    new() { Title = "Volatilidad en mercados asiáticos afecta criptomonedas", Source = "WSJ", TimeAgo = "Hace 8h", Sentiment = "Negativo" }
                }
            };

            return View(model);
        }

        private async Task<List<AssetSummary>> GetRealMarketData()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=10&page=1&sparkline=false&price_change_percentage=24h");

                // 🛡️ Usamos la variable privada que cargamos en el constructor
                request.Headers.Add("x-cg-demo-api-key", _coinGeckoKey);
                request.Headers.Add("User-Agent", "KairosAI-Dashboard/2.5");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode) return new List<AssetSummary>();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var assets = new List<AssetSummary>();

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    assets.Add(new AssetSummary
                    {
                        Symbol = element.GetProperty("symbol").GetString().ToUpper(),
                        Name = element.GetProperty("name").GetString(),
                        Price = element.GetProperty("current_price").GetDecimal(),
                        Change24h = element.GetProperty("price_change_percentage_24h").GetDouble(),
                        ImageUrl = element.GetProperty("image").GetString(),
                        RiskLevel = element.GetProperty("market_cap_rank").GetInt32() <= 3 ? "Bajo" : "Medio"
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