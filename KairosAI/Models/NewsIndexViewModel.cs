using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    // ═══════════════════════════════════════════
    //  NOTICIAS
    // ═══════════════════════════════════════════

    public class NewsIndexViewModel
    {
        public List<NewsItem> Headlines { get; set; } = new();

        /// <summary>
        /// Ticker de precios para el banner superior
        /// </summary>
        public List<MarketTickerItem> MarketTicker { get; set; } = new();
    }

    public class NewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Source { get; set; } = "";
        public string Summary { get; set; } = "";
        public string TimeAgo { get; set; } = "";
        public string Sentiment { get; set; } = "Neutro"; // "Positivo", "Neutro", "Negativo"
        public string Url { get; set; } = "#";
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "default"; // "cripto", "forex", "acciones", "macro"
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Activos financieros relacionados con esta noticia, con precio en tiempo real
        /// </summary>
        public List<RelatedAssetPrice> RelatedAssets { get; set; } = new();
    }

    /// <summary>
    /// Precio de un activo asociado a una noticia
    /// </summary>
    public class RelatedAssetPrice
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Change24h { get; set; }
        public string AssetType { get; set; } = "crypto"; // "crypto" | "stock"
        public string? ImageUrl { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public long Volume { get; set; }
        public bool IsMarketOpen { get; set; }

        /// <summary>
        /// Datos de sparkline para mini gráfico (últimas 24 data points)
        /// </summary>
        public List<double> SparklineData { get; set; } = new();

        // Propiedades calculadas
        public bool IsPositive => Change24h >= 0;
        public string ChangeFormatted => $"{(IsPositive ? "+" : "")}{Change24h:F2}%";
        public string PriceFormatted => Price >= 1 ? $"${Price:N2}" : $"${Price:F6}";
    }

    /// <summary>
    /// Item del ticker/banner de precios en tiempo real
    /// </summary>
    public class MarketTickerItem
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Change24h { get; set; }
        public string? ImageUrl { get; set; }
        public string AssetType { get; set; } = "crypto";

        public bool IsPositive => Change24h >= 0;
        public string ChangeFormatted => $"{(IsPositive ? "+" : "")}{Change24h:F2}%";
        public string PriceFormatted => Price >= 1 ? $"${Price:N2}" : $"${Price:F6}";
    }

    // ═══════════════════════════════════════════
    //  RECOMPENSAS (TIENDA) — Sin cambios
    // ═══════════════════════════════════════════

    public class RewardsIndexViewModel
    {
        public int UserPoints { get; set; }
        public List<RewardItem> Rewards { get; set; } = new();
    }

    public class RewardItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Cost { get; set; }
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public bool CanAfford(int currentPoints) => currentPoints >= Cost;
    }
}