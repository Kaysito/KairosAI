using Microsoft.AspNetCore.Mvc;
using KairosAI.Models.ViewModels;

namespace KairosAI.Controllers
{
    public class ProfileController : Controller
    {
        // GET: /Profile/Index
        public IActionResult Index()
        {
            var model = GetUserProfileData();
            return View(model);
        }

        /// <summary>
        /// Simula la recuperación de datos del usuario desde la BD o servicio
        /// TODO: Reemplazar con llamada real a servicio/repositorio
        /// </summary>
        private ProfileViewModel GetUserProfileData()
        {
            // Recuperar preferencia de tema
            string currentTheme = Request.Cookies["KairosTheme"] ?? "Dark";

            // Simular datos del usuario autenticado
            // En producción, esto vendría de: _userService.GetCurrentUser() o similar
            return new ProfileViewModel
            {
                FullName = "María González Rodríguez",
                Email = "maria.gonzalez@kairos.ai",
                ThemePreference = currentTheme,
                PreferredCurrency = "MXN",
                EmailNotifications = true,

                // Datos calculados del perfil Kairos
                RiskProfileLabel = "Moderado",
                RiskScore = 55,
                MemberSince = "15 de Noviembre, 2024",
                SubscriptionPlan = "Plan Gratuito"
            };
        }

        // POST: /Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ProfileViewModel model)
        {
            // Validación del modelo
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor, corrige los errores en el formulario antes de continuar.";

                // Repoblar datos de solo lectura que no vienen del form
                model.RiskProfileLabel = "Moderado";
                model.RiskScore = 55;
                model.MemberSince = "15 de Noviembre, 2024";
                model.SubscriptionPlan = "Plan Gratuito";
                model.ThemePreference = Request.Cookies["KairosTheme"] ?? "Dark";

                return View("Index", model);
            }

            // TODO: Aquí iría la lógica real de guardado
            // Ejemplo: await _userService.UpdateProfileAsync(model);

            // Simular éxito del guardado
            TempData["SuccessMessage"] = "✓ Tu perfil se actualizó correctamente.";

            return RedirectToAction("Index");
        }

        // POST: /Profile/ToggleTheme
        /// <summary>
        /// Alterna entre tema oscuro y claro, guardando la preferencia en cookie
        /// Puede ser llamado desde cualquier página que tenga el botón de tema
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTheme(string returnUrl = null)
        {
            // Leer tema actual desde cookie
            var currentTheme = Request.Cookies["KairosTheme"] ?? "Dark";

            // Invertir el tema
            var newTheme = currentTheme == "Dark" ? "Light" : "Dark";

            // Configurar cookie segura por 1 año
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true,      // Previene acceso desde JavaScript (XSS)
                Secure = true,        // Solo HTTPS en producción
                SameSite = SameSiteMode.Strict, // Protección CSRF
                IsEssential = true    // Necesaria para funcionalidad básica
            };

            Response.Cookies.Append("KairosTheme", newTheme, cookieOptions);

            // Feedback al usuario
            TempData["SuccessMessage"] = $"✓ Tema cambiado a {(newTheme == "Dark" ? "Oscuro" : "Claro")}.";

            // Redirigir a la página de origen o al perfil
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        // GET: /Profile/RetakeTest
        /// <summary>
        /// Redirige al usuario a la evaluación de riesgo del onboarding
        /// Útil cuando el usuario quiere actualizar su perfil de inversión
        /// </summary>
        public IActionResult RetakeTest()
        {
            TempData["InfoMessage"] = "Vas a rehacer tu evaluación de riesgo. Responde con sinceridad según tu situación actual.";
            return RedirectToAction("RiskAssessment", "Onboarding");
        }

        // GET: /Profile/DeleteAccount (opcional - buena práctica UX)
        /// <summary>
        /// Muestra página de confirmación para eliminar cuenta
        /// TODO: Implementar vista y lógica de eliminación
        /// </summary>
        public IActionResult DeleteAccount()
        {
            // TODO: Crear vista de confirmación
            return View();
        }

        // POST: /Profile/ConfirmDelete (opcional)
        /// <summary>
        /// Procesa la eliminación de cuenta del usuario
        /// Debe solicitar contraseña para confirmar
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmDelete(string password)
        {
            // TODO: Validar contraseña
            // TODO: Soft delete o hard delete según política
            // TODO: Enviar email de confirmación

            TempData["InfoMessage"] = "Tu cuenta ha sido eliminada. ¡Esperamos verte de nuevo!";
            return RedirectToAction("Index", "Home");
        }
    }
}