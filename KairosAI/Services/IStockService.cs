using KairosAI.Models.Api;

namespace KairosAI.Services
{
    public interface IStockService
    {
        /// Obtiene cotizaciones de acciones/índices
        Task<List<StockQuote>> GetQuotesAsync(string[] tickers);

        /// Obtiene noticias financieras de StockData.org
        Task<List<StockDataNewsItem>> GetNewsAsync(
            string? search = null,
            string? tickers = null,
            int limit = 20,
            string language = "es");
    }
}