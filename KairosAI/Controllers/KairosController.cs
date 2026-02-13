using Microsoft.AspNetCore.Mvc;
using KairosAI.Models.ViewModels;

namespace KairosAI.Controllers
{
    public class KairosController : Controller
    {
        // MEMORIA TEMPORAL (Static):
        // Esto mantiene el chat vivo mientras no reinicies el servidor.
        private static List<ChatMessage> _chatHistory = new()
        {
            new ChatMessage
            {
                Role = "Kairos",
                Text = "Hola. Soy KairosAI. Mi objetivo es analizar datos objetivamente para ayudarte a tomar decisiones. No doy consejos financieros directos. ¿Qué activo analizamos hoy?"
            }
        };

        // GET: /Kairos/Index
        public IActionResult Index()
        {
            var model = new ChatViewModel
            {
                ConversationHistory = _chatHistory
            };
            return View(model);
        }

        // POST: /Kairos/Send
        [HttpPost]
        public IActionResult Send(ChatViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentInput)) return RedirectToAction("Index");

            // 1. Guardar mensaje del usuario
            _chatHistory.Add(new ChatMessage
            {
                Role = "User",
                Text = model.CurrentInput
            });

            // 2. Simular "Pensamiento" de la IA (Lógica de Respuesta)
            var aiResponse = GenerateAIResponse(model.CurrentInput);
            _chatHistory.Add(aiResponse);

            // 3. Limpiar input y recargar
            return RedirectToAction("Index");
        }

        // POST: /Kairos/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            _chatHistory.Clear();
            _chatHistory.Add(new ChatMessage
            {
                Role = "Kairos",
                Text = "Memoria reiniciada. ¿En qué puedo ayudarte ahora?"
            });
            return RedirectToAction("Index");
        }

        // --- MOTOR DE RESPUESTAS SIMULADO ---
        private ChatMessage GenerateAIResponse(string input)
        {
            input = input.ToLower();
            var response = new ChatMessage { Role = "Kairos" };

            if (input.Contains("hola") || input.Contains("buenas"))
            {
                response.Text = "Hola de nuevo. Estoy monitoreando el mercado. El volumen de trading ha subido un 5% en la última hora.";
            }
            else if (input.Contains("bitcoin") || input.Contains("btc"))
            {
                response.Text = "Analizando Bitcoin (BTC): El RSI indica sobrecompra en gráficos de 4h. Históricamente, esto precede a correcciones leves. Mantén atención en el soporte de los $62k.";
            }
            else if (input.Contains("comprar") || input.Contains("invertir") || input.Contains("recomiendas"))
            {
                response.Text = "Como IA, no puedo decirte 'compra' o 'vende'. Sin embargo, los datos muestran que este activo tiene una tendencia alcista sólida. Recuerda gestionar tu riesgo y nunca invertir dinero que necesites a corto plazo.";
                response.IsWarning = true; // Esto hará que el mensaje se vea diferente (ej. borde amarillo)
            }
            else if (input.Contains("riesgo") || input.Contains("miedo"))
            {
                response.Text = "Es normal sentir incertidumbre. Según tu perfil (Moderado), te sugiero diversificar. No pongas más del 5% de tu capital en un solo activo volátil.";
            }
            else
            {
                response.Text = $"He procesado tu consulta sobre '{input}'. Los indicadores técnicos son mixtos en este momento. Te sugiero revisar las noticias recientes en la sección de Mercado.";
            }

            return response;
        }
    }
}