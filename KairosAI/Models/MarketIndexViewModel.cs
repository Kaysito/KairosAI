using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    // 1. EL MODELO DEL DETALLE DEL ACTIVO
    public class AssetDetailViewModel
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double Change24h { get; set; }
        public string RiskLevel { get; set; }
        public bool IsTrending { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }

        public string MarketCap { get; set; }
        public string Volume24h { get; set; }
        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }

        public List<string> ChartLabels { get; set; }
        public List<decimal> ChartData { get; set; }

        public string KairosSentiment { get; set; }
        public string KairosAnalysis { get; set; }
        public decimal AmountToInvest { get; set; }

        // Aquí está la variable que fallaba. Al estar MarketAsset en el mismo namespace, ya no habrá error CS0246.
        public List<MarketAsset> TopAssets { get; set; } = new List<MarketAsset>();
    }

    // 2. EL MODELO DE LA VISTA PRINCIPAL DEL MERCADO
    public class MarketIndexViewModel
    {
        public string SearchTerm { get; set; }
        public string Filter { get; set; }
        public List<MarketAsset> Assets { get; set; } = new List<MarketAsset>();
    }

    // 3. LA CLASE MARKET ASSET (El objeto que C# no encontraba)
    public class MarketAsset
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double Change24h { get; set; }
        public string RiskLevel { get; set; }
        public bool IsTrending { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
    }
}