namespace KairosAI.Models
{
    public class DashboardViewModel
    {
        public decimal TotalBalance { get; set; }
        public double MonthlyReturn { get; set; } // Porcentaje de ganancia/pérdida (ej. 12.5)

        // --- SECCIÓN DE PERFIL DE USUARIO ---
        public int RiskScore { get; set; } // 0-100 (Viene del Test)
        public string RiskLabel { get; set; } // "Conservador", "Moderado", "Agresivo"

        // --- SECCIÓN DE GRÁFICAS ---
        // Lista de puntos para dibujar la línea de rendimiento
        public List<ChartDataPoint> PerformanceData { get; set; } = new();

        // --- SECCIÓN DE MERCADO ---
        public List<AssetSummary> TopAssets { get; set; } = new();

        // --- SECCIÓN DE NOTICIAS ---
        public List<NewsItem> RecentNews { get; set; } = new();
    }

    // Clase auxiliar para la gráfica
    public class ChartDataPoint
    {
        public string Date { get; set; } // "Lun", "Mar", etc.
        public decimal Value { get; set; }
    }

    // Clase auxiliar para las tarjetas de monedas/acciones
    public class AssetSummary
    {
        public string Symbol { get; set; } // BTC, NVDA
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double Change24h { get; set; }
        public string RiskLevel { get; set; } // "Alto", "Medio", "Bajo"
    }

    // Clase auxiliar para el feed de noticias
    public class NewsItem
    {
        public string Title { get; set; }
        public string Source { get; set; } // "Bloomberg", "Reuters"
        public string TimeAgo { get; set; } // "Hace 2h"
        public string Sentiment { get; set; } // "Positivo", "Neutro", "Negativo"
    }
}

