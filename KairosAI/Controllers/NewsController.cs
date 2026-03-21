using KairosAI.Models.ViewModels;
using KairosAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsAggregatorService _newsAggregator;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            INewsAggregatorService newsAggregator,
            ICryptoService cryptoService,
            ILogger<NewsController> logger)
        {
            _newsAggregator = newsAggregator;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        // GET: /News/Index
        public async Task<IActionResult> Index()
        {
            var model = await _newsAggregator.GetAggregatedNewsAsync();
            return View(model);
        }

        // GET: /News/Read/{id} — Redirige a la URL de la noticia
        public IActionResult Read(int id, string? url)
        {
            if (!string.IsNullOrEmpty(url))
                return Redirect(url);

            return RedirectToAction("Index");
        }

        // ═══════════════════════════════════════════
        //  API ENDPOINTS para actualizaciones AJAX
        // ═══════════════════════════════════════════

        /// <summary>
        /// Endpoint AJAX para refrescar precios sin recargar la página
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLivePrices()
        {
            try
            {
                var cryptos = await _cryptoService.GetMarketDataAsync(perPage: 10);

                var result = cryptos.Select(c => new
                {
                    symbol = c.Symbol.ToUpper(),
                    name = c.Name,
                    price = c.CurrentPrice,
                    change24h = c.PriceChangePercentage24h ?? 0,
                    image = c.Image,
                    sparkline = c.SparklineIn7d?.Price?.TakeLast(24).ToList() ?? new List<decimal>()
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener precios live");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Endpoint para refrescar noticias via AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RefreshNews()
        {
            try
            {
                var model = await _newsAggregator.GetAggregatedNewsAsync();
                return PartialView("_NewsGrid", model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al refrescar noticias");
                return PartialView("_NewsGrid", new NewsIndexViewModel());
            }
        }
    }
}