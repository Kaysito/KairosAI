using System.ComponentModel.DataAnnotations;
namespace KairosAI.Models

{
    public class RiskAssessmentViewModel
    {
        // Pregunta 1: Objetivo de Inversión
        [Required(ErrorMessage = "Selecciona un objetivo.")]
        public string InvestmentGoal { get; set; } // "Preservar", "Crecimiento", "Especulación"

        // Pregunta 2: Tolerancia a caídas
        [Required(ErrorMessage = "Esta pregunta es crucial.")]
        public string DropReaction { get; set; } // "Vender", "Esperar", "Comprar más"

        // Pregunta 3: Horizonte Temporal
        [Required]
        public string TimeHorizon { get; set; } // "Corto Plazo (<1 año)", "Medio", "Largo (>5 años)"

        // Propiedad calculada solo para mostrar (no se guarda en form)
        public int CalculatedScore { get; set; }
    }
}

