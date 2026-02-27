using System;
using System.Collections.Generic;

namespace KairosAI.Models
{
    public class DashboardViewModel
    {
        public decimal TotalBalance { get; set; }
        public double MonthlyReturn { get; set; }
        public int RiskScore { get; set; }
        public string RiskLabel { get; set; }
        public List<ChartDataPoint> PerformanceData { get; set; } = new();
        public List<AssetSummary> TopAssets { get; set; } = new();
        public List<NewsItem> RecentNews { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Date { get; set; }
        public decimal Value { get; set; }
    }

    public class AssetSummary
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double Change24h { get; set; }
        public string RiskLevel { get; set; }
        public string ImageUrl { get; set; } // Propiedad para los iconos
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Source { get; set; }
        public string TimeAgo { get; set; }
        public string Sentiment { get; set; }
    }
}