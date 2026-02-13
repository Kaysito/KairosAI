using KairosAI.Models;
using KairosAI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class AccountController : Controller
    {
        // --- MODO PROTOTIPO ---
        // Sin dependencias de base de datos por ahora.

        #region Registro (Flujo: Registro -> Test de Riesgo)

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            // 1. Validación básica
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                return View(model);
            }

            if (!model.TermsAccepted)
            {
                ModelState.AddModelError("", "Debes aceptar los términos.");
                return View(model);
            }

            // 2. Simulación de Éxito de Registro
            // Aquí se guardaría el usuario en la BD.

            // 3. REDIRECCIÓN CRÍTICA: 
            // El usuario nuevo SIEMPRE debe pasar por el perfilado de riesgo antes de ver el Dashboard.
            return RedirectToAction("RiskAssessment", "Onboarding");
        }

        #endregion

        #region Login (Flujo: Login -> Dashboard)

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Simulación de validación de credenciales...
            // if (user == null) ... error

            // 2. Redirección
            // Si es un login normal (email/pass), asumimos que el usuario YA hizo el test
            // en el pasado, así que puede pasar directo al sistema.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        #endregion

        #region External Login (Google/Facebook -> Test de Riesgo)

        // Esta acción es la que llaman los botones de "Google" y "Facebook" en tu vista Login
        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            // Lógica simulada de OAuth (Google/Facebook).
            // Normalmente aquí se pide el reto al proveedor externo.

            // CASO DE USO:
            // Como pediste que NO se salten el test, en este prototipo asumiremos
            // que si alguien entra con Google/Facebook es la primera vez (Onboarding).

            // Por lo tanto, los redirigimos también al Test de Riesgo.
            return RedirectToAction("RiskAssessment", "Onboarding");
        }

        #endregion

        #region Logout

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        #endregion
    }
}