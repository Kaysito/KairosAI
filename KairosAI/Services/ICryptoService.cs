using KairosAI.Models.Api;

namespace KairosAI.Services
{
    public interface ICryptoService
    {
        /// Obtiene precios de mercado de las principales criptomonedas
        Task<List<CoinGeckoMarketItem>> GetMarketDataAsync(
            string vsCurrency = "usd",
            int perPage = 10,
            string[]? ids = null);

        /// Obtiene las criptos trending en CoinGecko
        Task<CoinGeckoTrendingResponse?> GetTrendingAsync();

        /// Obtiene precio simple de una o varias monedas
        Task<Dictionary<string, decimal>> GetSimplePricesAsync(
            string[] ids,
            string vsCurrency = "usd");
    }
}