using KairosAI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class RewardsController : Controller
    {
        // GET: /Rewards/Index
        public IActionResult Index()
        {
            // Simulamos los puntos (en una app real vendrían de la BD)
            // Usamos TempData si venimos de completar una lección, o un valor fijo.
            int currentPoints = 1250;

            var model = new RewardsIndexViewModel
            {
                UserPoints = currentPoints,
                Rewards = new List<RewardItem>
                {
                    new() {
                        Id = 1,
                        Title = "Tarjeta Google Play $200",
                        Cost = 5000,
                        Icon = "Play",
                        Description = "Crédito para apps y juegos."
                    },
                    new() {
                        Id = 2,
                        Title = "Amazon Gift Card $100",
                        Cost = 3000,
                        Icon = "Shopping",
                        Description = "Compra lo que quieras en Amazon."
                    },
                    new() {
                        Id = 3,
                        Title = "Mes Premium Kairos",
                        Cost = 1000,
                        Icon = "Crown",
                        Description = "Desbloquea análisis profundos de IA por 30 días."
                    },
                    new() {
                        Id = 4,
                        Title = "Skin de Perfil Exclusiva",
                        Cost = 500,
                        Icon = "User",
                        Description = "Destaca en la comunidad con un borde dorado."
                    }
                }
            };

            return View(model);
        }

        // POST: /Rewards/Redeem
        [HttpPost]
        public IActionResult Redeem(int id)
        {
            // Simulamos la lógica de canje
            // 1. Verificar si el usuario tiene puntos (user.Points >= reward.Cost)
            // 2. Restar puntos
            // 3. Generar código de tarjeta o activar beneficio

            // Por ahora, solo mandamos un mensaje de éxito
            TempData["SuccessMessage"] = "¡Canje exitoso! Disfruta tu recompensa.";

            return RedirectToAction("Index");
        }
    }
}
