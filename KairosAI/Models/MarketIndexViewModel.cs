using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    // --- VISTA GENERAL (TABLA) ---
    public class MarketIndexViewModel
    {
        public string SearchTerm { get; set; }
        public string Filter { get; set; } // "Cripto", "Acciones", "Todos"
        public List<MarketAsset> Assets { get; set; } = new();
    }

    public class MarketAsset
    {
        public string Symbol { get; set; } // BTC
        public string Name { get; set; } // Bitcoin
        public decimal Price { get; set; }
        public double Change24h { get; set; }
        public string Type { get; set; } // "Cripto", "Accion", "ETF"
        public string RiskLevel { get; set; } // "Alto", "Medio", "Bajo"
        public bool IsTrending { get; set; } // Para ponerle un fueguito 🔥

        // ¡NUEVO! Para que la tabla se vea igual de profesional que el dashboard
        public string ImageUrl { get; set; }
    }

    // --- VISTA DETALLADA (GRÁFICA Y COMPRA) ---
    public class AssetDetailViewModel : MarketAsset
    {
        // Datos para la gráfica
        public List<decimal> ChartData { get; set; } = new();
        public List<string> ChartLabels { get; set; } = new();

        // Datos fundamentales
        public string MarketCap { get; set; }
        public string Volume24h { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }

        // El cerebro de la app: Análisis de IA
        public string KairosAnalysis { get; set; }
        public string KairosSentiment { get; set; } // "Alcista", "Bajista", "Neutral"

        // Para el formulario de compra
        public decimal AmountToInvest { get; set; }
    }
}