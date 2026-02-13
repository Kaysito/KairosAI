using Microsoft.AspNetCore.Mvc;
using KairosAI.Models.ViewModels;

namespace KairosAI.Controllers
{
    public class MarketController : Controller
    {
        // Simulamos una "Base de Datos" en memoria para que la búsqueda funcione
        private static readonly List<MarketAsset> _fakeDb = new()
        {
            new() { Symbol = "BTC", Name = "Bitcoin", Price = 64230, Change24h = 2.4, Type = "Cripto", RiskLevel = "Alto", IsTrending = true },
            new() { Symbol = "ETH", Name = "Ethereum", Price = 3450, Change24h = -1.2, Type = "Cripto", RiskLevel = "Alto", IsTrending = false },
            new() { Symbol = "NVDA", Name = "Nvidia Corp", Price = 950, Change24h = 5.1, Type = "Accion", RiskLevel = "Medio", IsTrending = true },
            new() { Symbol = "TSLA", Name = "Tesla Inc", Price = 175, Change24h = -3.5, Type = "Accion", RiskLevel = "Alto", IsTrending = false },
            new() { Symbol = "SPY", Name = "S&P 500 ETF", Price = 510, Change24h = 0.5, Type = "ETF", RiskLevel = "Bajo", IsTrending = false },
            new() { Symbol = "MSTR", Name = "MicroStrategy", Price = 1400, Change24h = 12.5, Type = "Accion", RiskLevel = "Extremo", IsTrending = true },
        };

        // GET: /Market
        public IActionResult Index(string filter = "Todos")
        {
            var assets = _fakeDb;

            // Lógica de filtrado simple
            if (filter != "Todos")
            {
                assets = _fakeDb.Where(a => a.Type == filter).ToList();
            }

            var model = new MarketIndexViewModel
            {
                Filter = filter,
                Assets = assets
            };

            return View(model);
        }

        // GET: /Market/Details/BTC
        public IActionResult Details(string id) // El "id" será el Symbol (BTC, ETH...)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            // Buscar en nuestra BD falsa
            var asset = _fakeDb.FirstOrDefault(a => a.Symbol == id.ToUpper());

            if (asset == null) return NotFound(); // O RedirectToAction("Index")

            // Construir el modelo detallado extendido
            var model = new AssetDetailViewModel
            {
                // Copiamos datos básicos
                Symbol = asset.Symbol,
                Name = asset.Name,
                Price = asset.Price,
                Change24h = asset.Change24h,
                Type = asset.Type,
                RiskLevel = asset.RiskLevel,

                // Datos simulados extra
                MarketCap = asset.Type == "Cripto" ? "$1.2 T" : "$2.3 T",
                Volume24h = "$45 B",
                High24h = asset.Price * 1.05m,
                Low24h = asset.Price * 0.95m,

                // Simulamos gráfica (7 días de precios)
                ChartLabels = new List<string> { "Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom" },
                ChartData = new List<decimal>
                {
                    asset.Price * 0.9m,
                    asset.Price * 0.92m,
                    asset.Price * 0.88m,
                    asset.Price * 0.95m,
                    asset.Price * 1.02m,
                    asset.Price
                },

                // Lógica de Kairos (Aquí es donde brilla tu idea)
                KairosSentiment = asset.Change24h > 0 ? "Alcista" : "Bajista",
                KairosAnalysis = asset.RiskLevel == "Alto"
                    ? $"Precaución: {asset.Name} muestra alta volatilidad. Los indicadores técnicos sugieren esperar un retroceso antes de entrar."
                    : $"{asset.Name} mantiene una tendencia estable. Es un buen candidato para estrategias de acumulación a largo plazo."
            };

            return View(model);
        }

        // POST: /Market/Trade
        [HttpPost]
        public IActionResult Trade(AssetDetailViewModel model)
        {
            // Aquí iría la lógica real de compra:
            // 1. Validar saldo del usuario
            // 2. Crear transacción en BD
            // 3. Restar saldo

            // Simulamos éxito
            TempData["SuccessMessage"] = $"¡Orden ejecutada! Has comprado ${model.AmountToInvest} de {model.Symbol}.";

            // Regresamos al Dashboard para que vea su "compra"
            return RedirectToAction("Index", "Dashboard");
        }
    }
}