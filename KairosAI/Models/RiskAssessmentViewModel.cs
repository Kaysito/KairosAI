using System.ComponentModel.DataAnnotations;

namespace KairosAI.Models
{
    /// <summary>
    /// ViewModel para el test de calibración de KairósAI.
    /// Mide dos ejes independientes que producen el nivel de riesgo final.
    ///
    ///   EJE A — Agresividad / Pasividad (comportamiento emocional)
    ///   EJE B — Nivel de conocimiento   (principiante → experto)
    ///
    /// Nivel de riesgo = f(EjeA, EjeB)  →  ver matriz en OnboardingController
    /// </summary>
    public class RiskAssessmentViewModel
    {
        // ── Eje A: Comportamiento conductual ──────────────────────────────
        /// <summary>Reacción ante una caída del 25%: "Panico" | "Espera" | "Oportunidad"</summary>
        [Required(ErrorMessage = "Esta pregunta es clave para tu perfil.")]
        public string Behavior1 { get; set; } = string.Empty;

        /// <summary>Instinto con dinero inesperado: "Seguridad" | "Balance" | "Riesgo"</summary>
        [Required(ErrorMessage = "Esta pregunta es clave para tu perfil.")]
        public string Behavior2 { get; set; } = string.Empty;

        // ── Eje B: Nivel de conocimiento ──────────────────────────────────
        /// <summary>Lectura de gráfico de velas: "A" (correcto) | "B" (incorrecto) | "C" (no sé)</summary>
        [Required(ErrorMessage = "Responde la pregunta del gráfico.")]
        public string KnowledgeChart { get; set; } = string.Empty;

        /// <summary>Concepto de ETF: "Correcto" | "Parcial" | "Incorrecto"</summary>
        [Required(ErrorMessage = "Selecciona una opción.")]
        public string KnowledgeConcept { get; set; } = string.Empty;

        // ── Horizonte temporal (mantenido del modelo anterior) ────────────
        /// <summary>"Corto" | "Medio" | "Largo"</summary>
        public string TimeHorizon { get; set; } = "Medio";

        // ── Preferencias de dashboard ─────────────────────────────────────
        /// <summary>IDs de CoinGecko separados por coma. Ej: "bitcoin,ethereum,solana"</summary>
        public string SelectedCryptos { get; set; } = string.Empty;

        /// <summary>Mercados adicionales: "Cripto,Acciones,Commodities"</summary>
        public string SelectedMarkets { get; set; } = string.Empty;

        // ── Scores calculados en frontend, verificados en backend ─────────
        /// <summary>Puntaje Eje A: 2 (Pasivo) → 6 (Agresivo)</summary>
        public int AggressionScore { get; set; }

        /// <summary>Puntaje Eje B: 2 (Principiante) → 6 (Experto)</summary>
        public int KnowledgeScore { get; set; }

        // ── Output final ──────────────────────────────────────────────────
        /// <summary>Perfil calculado: "Inversor Cauteloso", "Explorador Prudente", etc.</summary>
        public string InvestmentGoal { get; set; } = string.Empty;

        /// <summary>Score compuesto 0-100 para persistir en BD</summary>
        public int CalculatedRiskScore { get; set; }
    }
}