using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // 1. RECUPERAR DATOS DEL TEST (Si viene de Onboarding)
            // Usamos un valor por defecto (50 - Moderado) si no hay dato previo.
            int userRiskScore = 50;

            if (TempData["UserRiskScore"] != null)
            {
                // Convertimos el objeto TempData a int
                if (int.TryParse(TempData["UserRiskScore"]?.ToString(), out int score))
                {
                    userRiskScore = score;
                }
            }

            // 2. CALCULAR ETIQUETA DE RIESGO
            string riskLabel = "Moderado";
            if (userRiskScore <= 30) riskLabel = "Conservador";
            else if (userRiskScore > 60) riskLabel = "Agresivo";

            // 3. GENERAR DATOS MOCK (Simulando una BD real)
            var model = new DashboardViewModel
            {
                TotalBalance = 245300.50m,
                MonthlyReturn = 12.5,
                RiskScore = userRiskScore,
                RiskLabel = riskLabel,

                // Simulación de gráfica (Últimos 7 días)
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

                // Activos destacados (Hardcoded basado en tu prototipo React)
                TopAssets = new List<AssetSummary>
                {
                    new() { Symbol = "BTC", Name = "Bitcoin", Price = 64230m, Change24h = 2.4, RiskLevel = "Alto" },
                    new() { Symbol = "ETH", Name = "Ethereum", Price = 3450m, Change24h = -1.2, RiskLevel = "Alto" },
                    new() { Symbol = "NVDA", Name = "Nvidia", Price = 950m, Change24h = 5.1, RiskLevel = "Medio" },
                    new() { Symbol = "USDT", Name = "Tether", Price = 1.00m, Change24h = 0.01, RiskLevel = "Bajo" }
                },

                // Noticias simuladas
                RecentNews = new List<NewsItem>
                {
                    new() { Title = "El mercado reacciona a la IA", Source = "Bloomberg", TimeAgo = "Hace 2h", Sentiment = "Positivo" },
                    new() { Title = "Nuevas regulaciones cripto", Source = "CoinDesk", TimeAgo = "Hace 4h", Sentiment = "Neutro" },
                    new() { Title = "Inflación baja al 3%", Source = "Reuters", TimeAgo = "Hace 6h", Sentiment = "Positivo" }
                }
            };

            return View(model);
        }
    }
}

