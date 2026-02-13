using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    // --- NOTICIAS ---
    public class NewsIndexViewModel
    {
        public List<NewsItem> Headlines { get; set; } = new();
    }

    public class NewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; } // "Bloomberg", "CoinDesk"
        public string Summary { get; set; }
        public string TimeAgo { get; set; } // "Hace 2h"
        public string Sentiment { get; set; } // "Positivo", "Neutro", "Negativo"
        public string Url { get; set; } // Link a la noticia real
    }

    // --- RECOMPENSAS (TIENDA) ---
    public class RewardsIndexViewModel
    {
        public int UserPoints { get; set; } // Puntos disponibles del usuario
        public List<RewardItem> Rewards { get; set; } = new();
    }

    public class RewardItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Cost { get; set; }
        public string Icon { get; set; } // "Gift", "CreditCard", "Star"
        public string Description { get; set; }

        // Propiedad calculada: ¿Le alcanza al usuario?
        public bool CanAfford(int currentPoints) => currentPoints >= Cost;
    }
}