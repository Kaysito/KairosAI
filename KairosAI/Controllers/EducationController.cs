using KairosAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class EducationController : Controller
    {
        // GET: /Education/Index
        public IActionResult Index()
        {
            // Simulamos que el usuario tiene 1250 puntos (mismo dato que el Dashboard)
            var model = new EducationIndexViewModel
            {
                TotalPoints = 1250,
                Modules = new List<CourseModule>
                {
                    new() {
                        Id = 1,
                        Title = "Fundamentos de Inversión",
                        Description = "Aprende la diferencia entre Ahorro e Inversión y el poder del interés compuesto.",
                        Level = "Básico",
                        RewardPoints = 50,
                        Progress = 100,
                        IsLocked = false,
                        Icon = "Book"
                    },
                    new() {
                        Id = 2,
                        Title = "Psicología del Trading",
                        Description = "Cómo controlar el miedo (FOMO) y la avaricia al operar.",
                        Level = "Intermedio",
                        RewardPoints = 100,
                        Progress = 45,
                        IsLocked = false,
                        Icon = "Brain"
                    },
                    new() {
                        Id = 3,
                        Title = "Análisis Técnico Avanzado",
                        Description = "Dominando Velas Japonesas, RSI y Medias Móviles.",
                        Level = "Avanzado",
                        RewardPoints = 200,
                        Progress = 0,
                        IsLocked = true, // Bloqueado hasta terminar el anterior
                        Icon = "Chart"
                    }
                }
            };

            return View(model);
        }

        // GET: /Education/Lesson/2
        public IActionResult Lesson(int id)
        {
            // Simulación: Cargamos contenido dummy según el ID
            var model = new LessonViewModel
            {
                Id = id,
                Title = id == 1 ? "Fundamentos de Inversión" : "Psicología del Trading",
                RewardPoints = 50,
                IsCompleted = id == 1, // Simulamos que el 1 ya está hecho
                ContentHtml = @"
                    <h3>¿Qué es el interés compuesto?</h3>
                    <p>Es el interés sobre el interés. Es la razón por la que invertir temprano es mejor que invertir mucho.</p>
                    <div class='alert alert-info'>Recuerda: El tiempo es tu mejor activo.</div>
                    <h3>La Regla del 72</h3>
                    <p>Divide 72 entre tu tasa de retorno anual para saber cuántos años tardarás en duplicar tu dinero.</p>
                ",
                VideoUrl = "https://www.youtube.com/watch?v=hB7CDrVnNCs", // Rickroll (placeholder) o video educativo real
                NextLessonId = id == 1 ? 2 : (int?)null
            };

            return View(model);
        }

        // POST: /Education/Complete/2
        [HttpPost]
        public IActionResult Complete(int id)
        {
            // AQUÍ LÓGICA DE NEGOCIO FUTURA:
            // 1. Buscar usuario y sumar user.Points += lesson.Points
            // 2. Marcar lección como completada en BD
            // 3. Desbloquear siguiente lección

            TempData["SuccessMessage"] = "¡Lección completada! Has ganado +50 KP.";

            return RedirectToAction("Index");
        }
    }
}
