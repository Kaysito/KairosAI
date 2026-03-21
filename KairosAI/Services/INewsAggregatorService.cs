using KairosAI.Models.ViewModels;

namespace KairosAI.Services
{
    public interface INewsAggregatorService
    {
        /// Agrega noticias de todas las fuentes, les asigna sentimiento
        /// y les asocia precios de mercado en tiempo real
        Task<NewsIndexViewModel> GetAggregatedNewsAsync();
    }
}