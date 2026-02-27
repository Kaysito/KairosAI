using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class OnboardingController : Controller
    {
        // ─────────────────────────────────────────────
        // GET: /Onboarding/RiskAssessment
        // ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult RiskAssessment()
        {
            return View(new RiskAssessmentViewModel());
        }

        // ─────────────────────────────────────────────
        // POST: /Onboarding/RiskAssessment
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult RiskAssessment(RiskAssessmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ══════════════════════════════════════════════════════════════
            //  EJE A — AGRESIVIDAD / PASIVIDAD  (puntaje 2-6, re-calculado
            //  en backend para no depender solo del frontend)
            // ══════════════════════════════════════════════════════════════
            int axisA = 0;

            // Pregunta 1: Reacción ante caída
            axisA += model.Behavior1 switch
            {
                "Panico" => 1,   // Pasivo extremo
                "Espera" => 2,   // Moderado
                "Oportunidad" => 3,   // Agresivo
                _ => 1
            };

            // Pregunta 2: Instinto con liquidez
            axisA += model.Behavior2 switch
            {
                "Seguridad" => 1,
                "Balance" => 2,
                "Riesgo" => 3,
                _ => 1
            };
            // axisA: 2 (muy pasivo) → 6 (muy agresivo)

            // ══════════════════════════════════════════════════════════════
            //  EJE B — CONOCIMIENTO / NIVEL  (puntaje 2-6)
            // ══════════════════════════════════════════════════════════════
            int axisB = 0;

            // Pregunta 3: Lectura de gráfico de velas
            // La respuesta correcta es "A" (tendencia alcista)
            axisB += model.KnowledgeChart switch
            {
                "A" => 3,   // Correcto — sabe leer velas
                "B" => 2,   // Parcialmente correcto
                "C" => 1,   // No sabe
                _ => 1
            };

            // Pregunta 4: Concepto ETF
            axisB += model.KnowledgeConcept switch
            {
                "Correcto" => 3,
                "Parcial" => 2,
                "Incorrecto" => 1,
                _ => 1
            };
            // axisB: 2 (principiante) → 6 (experto)

            // ══════════════════════════════════════════════════════════════
            //  MATRIZ DE RIESGO  f(axisA, axisB)
            //
            //                 Pasivo(2)  Mod(3-4)  Agresivo(5-6)
            //  Principiante:  Bajo 20%   Mod 45%   Alto 80% ⚠️
            //  Intermedio:    Bajo 25%   Mod 55%   Alto 75%
            //  Experto:       Bajo 30%   Mod 60%   Elevado 90%
            //
            //  Nota: el experto agresivo recibe MENOS fricción positiva
            //  que el principiante agresivo, aunque su riesgo sea mayor,
            //  porque entiende las consecuencias.
            // ══════════════════════════════════════════════════════════════
            int riskScore;
            string riskProfile;
            int frictionLevel; // 1=bajo, 2=medio, 3=alto — para la IA

            bool isAggressive = axisA >= 5;
            bool isModerate = axisA >= 3 && axisA <= 4;
            bool isPassive = axisA <= 2;

            bool isExpert = axisB >= 5;
            bool isIntermediate = axisB >= 3 && axisB <= 4;
            bool isBeginner = axisB <= 2;

            if (isPassive)
            {
                riskScore = 20;
                riskProfile = "Inversor Cauteloso";
                frictionLevel = 3; // Máxima protección
            }
            else if (isModerate && isBeginner)
            {
                riskScore = 45;
                riskProfile = "Explorador Prudente";
                frictionLevel = 3;
            }
            else if (isModerate && isIntermediate)
            {
                riskScore = 55;
                riskProfile = "Estratega Equilibrado";
                frictionLevel = 2;
            }
            else if (isModerate && isExpert)
            {
                riskScore = 60;
                riskProfile = "Estratega Equilibrado";
                frictionLevel = 2;
            }
            else if (isAggressive && isBeginner)
            {
                riskScore = 80;
                riskProfile = "Principiante Agresivo";
                frictionLevel = 3; // Máxima fricción positiva — sabe poco, arriesga mucho
            }
            else if (isAggressive && isIntermediate)
            {
                riskScore = 75;
                riskProfile = "Operador de Alto Vuelo";
                frictionLevel = 2;
            }
            else // isAggressive && isExpert
            {
                riskScore = 90;
                riskProfile = "Operador de Alto Vuelo";
                frictionLevel = 1; // Fricción mínima — sabe lo que hace
            }

            // Bonus: horizonte temporal largo suma 5 puntos al riesgo permitido
            if (model.TimeHorizon == "Largo")
                riskScore = Math.Min(riskScore + 5, 100);

            // ══════════════════════════════════════════════════════════════
            //  GUARDAR EN SESSION / TempData (BD en fases futuras)
            // ══════════════════════════════════════════════════════════════
            TempData["UserRiskScore"] = riskScore;
            TempData["UserRiskProfile"] = riskProfile;
            TempData["UserAggressionAxis"] = axisA;
            TempData["UserKnowledgeAxis"] = axisB;
            TempData["FrictionLevel"] = frictionLevel;
            TempData["SelectedCryptos"] = model.SelectedCryptos;
            TempData["SelectedMarkets"] = model.SelectedMarkets;

            // En producción se persistiría así:
            // var user = await _userManager.GetUserAsync(User);
            // user.RiskScore      = riskScore;
            // user.RiskProfile    = riskProfile;
            // user.AggressionAxis = axisA;
            // user.KnowledgeAxis  = axisB;
            // user.FrictionLevel  = frictionLevel;
            // user.WatchedCryptos = model.SelectedCryptos;
            // await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}