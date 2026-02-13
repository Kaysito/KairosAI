using KairosAI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KairosAI.Controllers
{
    public class NewsController : Controller
    {
        // GET: /News/Index
        public IActionResult Index()
        {
            var model = new NewsIndexViewModel
            {
                Headlines = new List<NewsItem>
                {
                    new() {
                        Id = 1,
                        Title = "El S&P 500 alcanza máximos históricos tras anuncio de la FED",
                        Source = "Bloomberg",
                        Summary = "Jerome Powell sugiere que los recortes de tasas podrían llegar antes de lo esperado.",
                        TimeAgo = "Hace 2h",
                        Sentiment = "Positivo"
                    },
                    new() {
                        Id = 2,
                        Title = "Bitcoin enfrenta resistencia en los $68k",
                        Source = "CoinDesk",
                        Summary = "Los mineros están vendiendo reservas, lo que podría generar presión bajista a corto plazo.",
                        TimeAgo = "Hace 4h",
                        Sentiment = "Neutro"
                    },
                    new() {
                        Id = 3,
                        Title = "Regulaciones estrictas para IA en Europa",
                        Source = "Reuters",
                        Summary = "Las acciones tecnológicas podrían verse afectadas por las nuevas normativas de la UE.",
                        TimeAgo = "Hace 6h",
                        Sentiment = "Negativo"
                    },
                     new() {
                        Id = 4,
                        Title = "Tesla presenta su nuevo Robotaxi",
                        Source = "TechCrunch",
                        Summary = "Los inversores reaccionan con escepticismo ante los plazos de entrega.",
                        TimeAgo = "Hace 8h",
                        Sentiment = "Negativo"
                    }
                }
            };

            return View(model);
        }

        // Opcional: Acción para leer noticia completa
        public IActionResult Read(int id)
        {
            // Aquí redirigiríamos a la URL real o mostraríamos detalle
            return RedirectToAction("Index");
        }
    }
}
