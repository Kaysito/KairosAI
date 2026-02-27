using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

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
            _coinGeckoKey = _config["ApiKeys:CoinGecko"];
        }

        public async Task<IActionResult> Index()
        {
            // 1. RECUPERAR DATOS DEL TEST (O valores por defecto para la demo)
            // Agresividad: 2 (Pasivo) a 6 (Muy Agresivo)
            int aggression = TempData["UserAggressionAxis"] != null ? (int)TempData["UserAggressionAxis"] : 4;

            // Conocimiento: 2 (Principiante) a 6 (Experto)
            int knowledge = TempData["UserKnowledgeAxis"] != null ? (int)TempData["UserKnowledgeAxis"] : 3;

            // 2. DETERMINAR PERFIL PSICOLÓGICO Y FRICCIÓN (Lógica de Negocio Kairós)
            string riskProfile = "Inversor Equilibrado";
            int frictionLevel = 2; // Moderado por defecto

            if (aggression <= 3 && knowledge <= 3)
            {
                riskProfile = "Inversor Cauteloso";
                frictionLevel = 3; // Escudo máximo para principiantes pasivos
            }
            else if (aggression >= 5 && knowledge <= 3)
            {
                riskProfile = "Principiante Agresivo";
                frictionLevel = 3; // Escudo máximo: mucho riesgo, poca experiencia
            }
            else if (aggression >= 5 && knowledge >= 5)
            {
                riskProfile = "Inversor de Alto Vuelo";
                frictionLevel = 1; // Escudo mínimo: sabe lo que hace y es agresivo
            }
            else if (knowledge >= 4)
            {
                riskProfile = "Explorador Informado";
                frictionLevel = 2;
            }

            // 3. OBTENER DATOS REALES DE MERCADO
            var realAssets = await GetRealMarketData();

            // 4. GENERAR EL MODELO FINAL
            var model = new DashboardViewModel
            {
                TotalBalance = 245300.50m,
                MonthlyReturn = 12.5,

                // Mapeo de ejes para la vista fluida
                AggressionAxis = aggression,
                KnowledgeAxis = knowledge,
                RiskProfile = riskProfile,
                FrictionLevel = frictionLevel,
                RiskLabel = riskProfile, // Sincronizado para compatibilidad

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