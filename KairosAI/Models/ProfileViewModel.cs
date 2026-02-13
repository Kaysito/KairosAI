using System.ComponentModel.DataAnnotations;

namespace KairosAI.Models.ViewModels
{
    public class ProfileViewModel
    {
        // --- DATOS PERSONALES ---
        [Display(Name = "Nombre Completo")]
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FullName { get; set; }

        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido")]
        public string Email { get; set; }

        // --- PREFERENCIAS DE APP ---
        [Display(Name = "Tema Visual")]
        public string ThemePreference { get; set; } // "Dark" o "Light"

        [Display(Name = "Moneda Base")]
        public string PreferredCurrency { get; set; } // "MXN", "USD"

        [Display(Name = "Notificaciones por Correo")]
        public bool EmailNotifications { get; set; }

        // --- ESTADO KAIROS (Solo lectura) ---
        public string RiskProfileLabel { get; set; }
        public int RiskScore { get; set; }
        public string MemberSince { get; set; }
        public string SubscriptionPlan { get; set; }

        // --- PROPIEDADES AUXILIARES PARA LA VISTA ---
        public string RiskProfileColor => RiskScore switch
        {
            <= 30 => "bg-green-500",
            <= 60 => "bg-yellow-500",
            _ => "bg-red-500"
        };

        public string RiskProfileIcon => RiskScore switch
        {
            <= 30 => "🛡️",
            <= 60 => "⚖️",
            _ => "⚡"
        };

        public string RiskProfileDescription => RiskScore switch
        {
            <= 30 => "Prefieres inversiones seguras y estables",
            <= 60 => "Equilibras seguridad con oportunidades de crecimiento",
            _ => "Buscas alto rendimiento con mayor volatilidad"
        };
    }
}