using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json; // Esencial para guardar listas de forma segura

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
            // 1. Verificación de errores en el formulario
            if (!ModelState.IsValid)
            {
                // Si la pantalla se queda aquí, falta algún dato obligatorio del modelo en tu HTML
                return View(model);
            }

            // ══════════════════════════════════════════════════════════════
            //  EJE A — AGRESIVIDAD / PASIVIDAD 
            // ══════════════════════════════════════════════════════════════
            int axisA = 0;

            // Pregunta 1: Reacción ante caída
            axisA += model.Behavior1 switch
            {
                "Panico" => 1,
                "Espera" => 2,
                "Oportunidad" => 3,
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

            // ══════════════════════════════════════════════════════════════
            //  EJE B — CONOCIMIENTO / NIVEL
            // ══════════════════════════════════════════════════════════════
            int axisB = 0;

            // Pregunta 3: Lectura de gráfico de velas
            axisB += model.KnowledgeChart switch
            {
                "A" => 3,
                "B" => 2,
                "C" => 1,
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

            // ══════════════════════════════════════════════════════════════
            //  MATRIZ DE RIESGO  f(axisA, axisB)
            // ══════════════════════════════════════════════════════════════
            int riskScore;
            string riskProfile;
            int frictionLevel;

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
                frictionLevel = 3;
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
                frictionLevel = 3;
            }
            else if (isAggressive && isIntermediate)
            {
                riskScore = 75;
                riskProfile = "Operador de Alto Vuelo";
                frictionLevel = 2;
            }
            else
            {
                riskScore = 90;
                riskProfile = "Operador de Alto Vuelo";
                frictionLevel = 1;
            }

            // Bonus: horizonte temporal largo suma 5 puntos al riesgo permitido
            if (model.TimeHorizon == "Largo")
                riskScore = Math.Min(riskScore + 5, 100);

            // ══════════════════════════════════════════════════════════════
            //  GUARDAR EN TEMPDATA (Con serialización segura para listas)
            // ══════════════════════════════════════════════════════════════
            TempData["UserRiskScore"] = riskScore;
            TempData["UserRiskProfile"] = riskProfile;
            TempData["UserAggressionAxis"] = axisA;
            TempData["UserKnowledgeAxis"] = axisB;
            TempData["FrictionLevel"] = frictionLevel;

            // 🔥 SOLUCIÓN DEL BUG: Convertir a JSON las listas para que TempData no crashee
            TempData["SelectedCryptos"] = model.SelectedCryptos ?? string.Empty;
            TempData["SelectedMarkets"] = model.SelectedMarkets ?? string.Empty;

            // ══════════════════════════════════════════════════════════════
            //  REDIRECCIÓN AL DASHBOARD
            // ══════════════════════════════════════════════════════════════
            return RedirectToAction("Index", "Dashboard");
        }
    }
}