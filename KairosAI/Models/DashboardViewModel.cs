using System;
using System.Collections.Generic;

namespace KairosAI.Models
{
    public class DashboardViewModel
    {
        public decimal TotalBalance { get; set; }
        public double MonthlyReturn { get; set; }

        // Propiedades originales (compatibilidad)
        public int RiskScore { get; set; }
        public string RiskLabel { get; set; }

        // ── NUEVAS PROPIEDADES PARA PERFILADO PSICOLÓGICO ──
        // Eje A: Agresividad (Valor de 2 a 6 para el cálculo de porcentaje)
        public int AggressionAxis { get; set; }

        // Eje B: Conocimiento/Experiencia (Valor de 2 a 6)
        public int KnowledgeAxis { get; set; }

        // Perfil descriptivo (ej: "Inversor Equilibrado")
        public string RiskProfile { get; set; }

        // Nivel de intervención de la IA (1: Mínimo, 2: Moderado, 3: Máximo)
        public int FrictionLevel { get; set; }

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
        public string ImageUrl { get; set; }
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Source { get; set; }
        public string TimeAgo { get; set; }
        public string Sentiment { get; set; }
    }
}