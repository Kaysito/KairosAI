using System;
using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    public class ChatViewModel
    {
        public List<ChatMessage> ConversationHistory { get; set; } = new();
        public string CurrentInput { get; set; } = string.Empty;

        // Datos dinámicos para la vista
        public string UserDisplayName { get; set; } = "Usuario";
        public string UserInitials { get; set; } = "U";
        public string AppVersion { get; set; } = "2.4";

        // Suggestion chips configurables desde el controlador
        public List<SuggestionChip> SuggestionChips { get; set; } = new()
        {
            new("📈", "Rendimiento", "portfolio",
                "¿Cómo va mi portafolio este mes?",
                "Analiza mi rendimiento mensual"),
            new("📚", "Academia", "academy",
                "¿Qué es un ETF de Bitcoin?",
                "¿Qué es un ETF de Bitcoin?"),
            new("🎯", "Estrategia", "strategy",
                "¿Cuál es mi perfil de riesgo?",
                "¿Cuál es mi perfil de riesgo actual?"),
            new("⚡", "Mercado", "market",
                "Noticias crypto de hoy",
                "Dame noticias del mercado crypto hoy"),
        };
    }

    public class SuggestionChip
    {
        public string Icon { get; set; }
        public string Label { get; set; }
        public string CssModifier { get; set; }
        public string DisplayText { get; set; }
        public string Prompt { get; set; }

        public SuggestionChip() { }

        public SuggestionChip(string icon, string label,
            string cssModifier, string displayText, string prompt)
        {
            Icon = icon;
            Label = label;
            CssModifier = cssModifier;
            DisplayText = displayText;
            Prompt = prompt;
        }
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Role { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsWarning { get; set; }
    }
}