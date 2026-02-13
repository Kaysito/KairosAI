using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class OnboardingController : Controller
    {
        // GET: /Onboarding/RiskAssessment
        [HttpGet]
        public IActionResult RiskAssessment()
        {
            return View(new RiskAssessmentViewModel());
        }

        // POST: /Onboarding/RiskAssessment
        [HttpPost]
        public IActionResult RiskAssessment(RiskAssessmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --- LÓGICA DE CÁLCULO DE RIESGO (SIMULADA) ---

            int score = 0;

            // 1. Evaluar Objetivo
            if (model.InvestmentGoal == "Preservar") score += 10;
            else if (model.InvestmentGoal == "Crecimiento") score += 20;
            else if (model.InvestmentGoal == "Especulacion") score += 30;

            // 2. Evaluar Reacción a Caídas (La más importante)
            if (model.DropReaction == "Vender") score += 0; // Miedo alto = Perfil bajo
            else if (model.DropReaction == "Esperar") score += 20;
            else if (model.DropReaction == "Comprar") score += 40; // Agresivo

            // 3. Evaluar Tiempo
            if (model.TimeHorizon == "Corto") score += 5;
            else if (model.TimeHorizon == "Largo") score += 30; // Más tiempo = Más capacidad de riesgo

            // Normalizamos a 100 por si acaso
            if (score > 100) score = 100;

            // En un futuro, aquí guardaríamos 'score' en la Base de Datos del usuario:
            // user.RiskScore = score;
            // await _userManager.UpdateAsync(user);

            // Redirigimos al Dashboard, enviando el puntaje como dato temporal (TempData)
            // para mostrar un mensaje de bienvenida personalizado.
            TempData["UserRiskScore"] = score;

            return RedirectToAction("Index", "Dashboard");
        }
    }
}

