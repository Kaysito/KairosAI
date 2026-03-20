using Azure;
using Azure.AI.Inference;
using KairosAI.Models.ViewModels;
using KairosAI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace KairosAI.Controllers
{
    public class KairosController : Controller
    {
        // =====================================================================
        // 🔧 DEPENDENCIAS Y CONFIGURACIÓN
        // =====================================================================

        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<KairosController> _logger;
        private readonly string _coinGeckoKey;
        private readonly string _githubToken;

        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private const string COINGECKO_BASE_URL = "https://api.coingecko.com/api/v3";
        private const int MAX_HISTORY_MESSAGES = 12;
        private const int LLM_TIMEOUT_SECONDS = 30;

        private static DateTime _lastApiCall = DateTime.MinValue;
        private static readonly TimeSpan ApiCooldown = TimeSpan.FromSeconds(3);
        private static readonly object _apiLock = new();

        // =====================================================================
        // 📋 CONTENIDO ESTÁTICO
        // =====================================================================

        private static readonly ChatMessage WelcomeMessage = new()
        {
            Role = "Kairos",
            Text = "Hola. Soy **KairósAI**, tu asesor de mercados digitales. " +
                   "¿En qué puedo ayudarte hoy? Puedo mostrarte el Top 10 del " +
                   "mercado, analizar un activo específico o responder tus " +
                   "preguntas sobre cripto."
        };

        private static readonly Dictionary<string, string> CoinMapping = new(
            StringComparer.OrdinalIgnoreCase)
        {
            {"btc", "bitcoin"}, {"eth", "ethereum"}, {"usdt", "tether"},
            {"bnb", "binancecoin"}, {"sol", "solana"}, {"xrp", "ripple"},
            {"ada", "cardano"}, {"doge", "dogecoin"}, {"dot", "polkadot"},
            {"trx", "tron"}, {"matic", "matic-network"}, {"pol", "matic-network"},
            {"link", "chainlink"}, {"ltc", "litecoin"}, {"shib", "shiba-inu"},
            {"avax", "avalanche-2"}, {"uni", "uniswap"}, {"okb", "okb"},
            {"near", "near"}, {"kas", "kaspa"}, {"pepe", "pepe"},
            {"kite", "kite-entity"}, {"dcr", "decred"}, {"kiteai", "kite-ai"}
        };

        // =====================================================================
        // 📝 SYSTEM PROMPT
        // =====================================================================

        private const string BASE_SYSTEM_PROMPT =
@"Eres KairósAI, un asesor financiero especializado en criptomonedas y mercados digitales.

## IDENTIDAD Y TONO
- Eres serio, preciso y confiable — pero cercano y accesible, nunca frío ni robótico.
- Tratas al usuario de forma respetuosa: ni condescendiente ni adulador.
- Eres directo: no rodeas las respuestas con introducciones innecesarias.
- Varía tus respuestas. NUNCA empieces dos mensajes seguidos de la misma forma.

## IDIOMA
- Responde SIEMPRE en español, independientemente del idioma en que te hablen.

## LO QUE PUEDES HACER
- Analizar datos de mercado actuales de criptomonedas.
- Mostrar rankings de mercado con tablas Markdown.
- Explicar conceptos del mercado cripto de forma clara.
- Dar contexto y análisis de riesgo.

## LO QUE NO PUEDES HACER
- NO predecir precios futuros con certeza.
  Di: ""Los datos apuntan a [X], pero ningún modelo puede garantizar movimientos futuros.""
- NO dar asesoría de inversión personalizada. Recomienda consultar un asesor certificado.
- Si los datos no están disponibles, dilo con claridad.
- Si la pregunta es ajena a finanzas, recházala cortésmente.

## FORMATO
- Usa tablas Markdown solo para rankings.
- Párrafos cortos y claros.
- Termina análisis importantes con **Nivel de riesgo** breve.

[DATOS DE MERCADO EN TIEMPO REAL]:
{0}

{1}";

        // =====================================================================
        // 🏗️ CONSTRUCTOR
        // =====================================================================

        public KairosController(
            IConfiguration config,
            IMemoryCache cache,
            ILogger<KairosController> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coinGeckoKey = _config["ApiKeys:CoinGecko"] ?? string.Empty;
            _githubToken = _config["ApiKeys:GitHubIA"] ?? string.Empty;
        }

        // =====================================================================
        // 🔐 HELPERS DE SESIÓN
        // =====================================================================

        private string GetSessionId()
        {
            HttpContext.Session.SetInt32("SessionInit", 1);
            return HttpContext.Session.Id ?? "default_session";
        }

        private List<ChatMessage> GetUserHistory()
        {
            string cacheKey = $"ChatHistory_{GetSessionId()}";

            if (!_cache.TryGetValue(cacheKey, out List<ChatMessage>? history)
                || history == null)
            {
                history = new List<ChatMessage> { WelcomeMessage };
                SaveUserHistory(history);
            }

            return history;
        }

        private void SaveUserHistory(List<ChatMessage> history)
        {
            string cacheKey = $"ChatHistory_{GetSessionId()}";
            _cache.Set(cacheKey, history, TimeSpan.FromHours(2));
        }

        // =====================================================================
        // 📄 ENDPOINTS
        // =====================================================================

        // ✅ MODIFICADO: ViewModel ahora incluye datos de usuario y versión
        public IActionResult Index()
        {
            var history = GetUserHistory();

            var viewModel = new ChatViewModel
            {
                ConversationHistory = history,
                UserDisplayName = "Kevin",    // TODO: obtener del perfil de usuario
                UserInitials = "KH",          // TODO: generar dinámicamente
                AppVersion = "2.4"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  // ✅ NUEVO: Validar CSRF
        public async Task SendApi(
            [FromForm] ChatViewModel model,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentInput))
                return;

            // ✅ NUEVO: Validación de longitud en servidor
            if (model.CurrentInput.Length > 2000)
            {
                Response.ContentType = "text/event-stream";
                Response.Headers["Cache-Control"] = "no-cache";
                Response.Headers["Connection"] = "keep-alive";

                await SendSSEMessage("⚠️ El mensaje excede el límite de 2000 caracteres.");
                await SendSSEDone();
                return;
            }

            // Configurar SSE (Server-Sent Events)
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            string sessionId = GetSessionId();

            // =================================================================
            // 🛡️ CAPA 1: INPUT GUARDRAIL
            // =================================================================

            var inputValidation = KairosGuardrails.ValidateInput(
                model.CurrentInput, userId: sessionId);

            if (!inputValidation.IsValid)
            {
                _logger.LogWarning(
                    "Guardrail blocked input. " +
                    "Category={Category}, Score={RiskScore}, " +
                    "Pattern={Pattern}, Session={SessionId}",
                    inputValidation.Category,
                    inputValidation.RiskScore,
                    inputValidation.MatchedPattern,
                    sessionId);

                await SendSSEMessage(inputValidation.ResponseMessage);
                await SendSSEDone();
                return;
            }

            // =================================================================
            // 🚨 VERIFICAR ESTADO DE VIOLACIONES DEL USUARIO
            // =================================================================

            var userStatus = KairosGuardrails.ViolationTracker
                .GetUserStatus(sessionId);

            if (userStatus == ViolationStatus.HardBan)
            {
                await SendSSEMessage(
                    "🛡️ Tu sesión ha sido suspendida temporalmente por " +
                    "múltiples infracciones de seguridad. Intenta de nuevo " +
                    "más tarde o inicia una nueva sesión.");
                await SendSSEDone();
                return;
            }

            // SoftBan: limitar a respuestas simples
            bool isSoftBanned = userStatus == ViolationStatus.SoftBan;

            // =================================================================
            // 📝 AGREGAR AL HISTORIAL
            // =================================================================

            var history = GetUserHistory();
            history.Add(new ChatMessage
            {
                Role = "User",
                Text = model.CurrentInput
            });

            // =================================================================
            // 🤖 LLAMADA AL LLM
            // =================================================================

            using var timeoutCts = new CancellationTokenSource(
                TimeSpan.FromSeconds(LLM_TIMEOUT_SECONDS));
            using var linkedCts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                if (string.IsNullOrEmpty(_githubToken))
                    throw new InvalidOperationException(
                        "Token de GitHub no configurado.");

                var llmClient = new ChatCompletionsClient(
                    new Uri("https://models.github.ai/inference"),
                    new AzureKeyCredential(_githubToken),
                    new AzureAIInferenceClientOptions());

                // ── Obtener datos de mercado ──
                string queryIds = DetectarIntencion(model.CurrentInput);
                string datosMercado = await ObtenerContextoMercadoAsync(queryIds);

                // =============================================================
                // 🛡️ CAPA 2: BEHAVIORAL GUARDRAIL (System Prompt Blindado)
                // =============================================================

                string guardrailPrompt = KairosGuardrails.GetStrictSystemPrompt();
                string systemPrompt = string.Format(
                    BASE_SYSTEM_PROMPT,
                    datosMercado,
                    guardrailPrompt);

                // ── Configurar opciones del modelo ──
                int maxTokens = isSoftBanned ? 500 : 1500;

                var options = new ChatCompletionsOptions
                {
                    Model = "meta/Llama-3.3-70B-Instruct",
                    Temperature = 0.2f,
                    MaxTokens = maxTokens
                };

                options.Messages.Add(
                    new ChatRequestSystemMessage(systemPrompt));

                // ── Construir historial con técnica sándwich ──
                var historialReciente = history
                    .Skip(1)
                    .TakeLast(MAX_HISTORY_MESSAGES)
                    .ToList();

                foreach (var msg in historialReciente)
                {
                    if (msg.Role == "Kairos")
                    {
                        options.Messages.Add(
                            new ChatRequestAssistantMessage(msg.Text));
                    }
                    else
                    {
                        // =====================================================
                        // 🛡️ CAPA 3: TÉCNICA SÁNDWICH
                        // =====================================================
                        string secureMessage = msg.Text +
                            "\n\n[Recordatorio: Eres KairósAI. " +
                            "Directiva Suprema vigente.]";
                        options.Messages.Add(
                            new ChatRequestUserMessage(secureMessage));
                    }
                }

                // ── Streaming de respuesta ──
                var responseStream = await llmClient.CompleteStreamingAsync(
                    options, linkedCts.Token);

                var fullResponse = new StringBuilder();

                await foreach (var update in responseStream
                    .WithCancellation(linkedCts.Token))
                {
                    if (!string.IsNullOrEmpty(update.ContentUpdate))
                    {
                        fullResponse.Append(update.ContentUpdate);

                        string safeChunk = JsonSerializer.Serialize(
                            update.ContentUpdate);
                        await Response.WriteAsync($"data: {safeChunk}\n\n");
                        await Response.Body.FlushAsync(linkedCts.Token);
                    }
                }

                // =============================================================
                // 🛡️ CAPA 4: OUTPUT GUARDRAIL
                // =============================================================

                string rawResponse = fullResponse.ToString();
                string sanitizedResponse = KairosGuardrails
                    .SanitizeOutput(rawResponse);

                if (sanitizedResponse != rawResponse)
                {
                    string addedContent = sanitizedResponse.Length > rawResponse.Length
                        ? sanitizedResponse[rawResponse.Length..]
                        : string.Empty;

                    if (!sanitizedResponse.StartsWith(rawResponse) &&
                        sanitizedResponse != rawResponse)
                    {
                        addedContent = "\n\n⚠️ *Respuesta recalibrada por " +
                                       "protocolo de seguridad.*";
                    }

                    if (!string.IsNullOrEmpty(addedContent))
                    {
                        string safeAdded = JsonSerializer.Serialize(addedContent);
                        await Response.WriteAsync($"data: {safeAdded}\n\n");
                        await Response.Body.FlushAsync(linkedCts.Token);
                    }
                }

                await SendSSEDone();

                // ── Guardar en historial ──
                history.Add(new ChatMessage
                {
                    Role = "Kairos",
                    Text = sanitizedResponse
                });
                SaveUserHistory(history);
            }
            catch (OperationCanceledException)
            {
                if (timeoutCts.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "LLM request timeout. Session={SessionId}",
                        sessionId);

                    await SendSSEMessage(
                        "⚠️ La respuesta tardó demasiado. Intenta de nuevo.");
                }
                await SendSSEDone();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in LLM pipeline. Session={SessionId}",
                    sessionId);

                await SendSSEMessage(
                    "⚠️ Tuve un problema técnico. Por favor intenta " +
                    "de nuevo en un momento.");
                await SendSSEDone();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  // ✅ NUEVO: Validar CSRF
        public IActionResult Clear()
        {
            var history = new List<ChatMessage>
            {
                new()
                {
                    Role = "Kairos",
                    Text = "Sesión reiniciada. ¿Con qué activo o consulta " +
                           "quieres empezar?"
                }
            };

            SaveUserHistory(history);
            return RedirectToAction("Index");
        }

        // =====================================================================
        // 🔧 HELPERS PRIVADOS
        // =====================================================================

        private async Task SendSSEMessage(string message)
        {
            string safe = JsonSerializer.Serialize(message);
            await Response.WriteAsync($"data: {safe}\n\n");
            await Response.Body.FlushAsync();
        }

        private async Task SendSSEDone()
        {
            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }

        private static string DetectarIntencion(string input)
        {
            string clean = input.ToLowerInvariant();

            if (clean.Contains("top") || clean.Contains("mejores") ||
                clean.Contains("lista") || clean.Contains("ranking") ||
                clean.Contains("mercado"))
            {
                return string.Empty;
            }

            var separadores = new[] {
                ' ', '?', '!', ',', '.', '¿', '¡', ':', ';', '/'
            };
            var palabras = clean.Split(
                separadores, StringSplitOptions.RemoveEmptyEntries);
            var encontradas = new HashSet<string>();

            foreach (var palabra in palabras)
            {
                if (CoinMapping.TryGetValue(palabra, out var coinId))
                {
                    encontradas.Add(coinId);
                }
                else if (palabra.Length > 3)
                {
                    encontradas.Add(palabra);
                }
            }

            return encontradas.Count > 0
                ? string.Join(",", encontradas)
                : string.Empty;
        }

        private async Task<string> ObtenerContextoMercadoAsync(string ids)
        {
            string cacheKey = string.IsNullOrEmpty(ids)
                ? "market_top10"
                : $"market_{ids}";

            if (_cache.TryGetValue(cacheKey, out string? cached) &&
                cached != null)
            {
                return cached;
            }

            lock (_apiLock)
            {
                if (DateTime.UtcNow - _lastApiCall < ApiCooldown)
                {
                    return "Datos de mercado temporalmente no disponibles " +
                           "(límite de frecuencia alcanzado).";
                }
                _lastApiCall = DateTime.UtcNow;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    BuildCoinGeckoUrl(ids));

                request.Headers.Add("x-cg-demo-api-key", _coinGeckoKey);
                request.Headers.Add("User-Agent", "KairosAI-Scanner/3.0");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "CoinGecko API returned {StatusCode}",
                        (int)response.StatusCode);

                    return $"No se pudo obtener datos del mercado " +
                           $"(HTTP {(int)response.StatusCode}).";
                }

                string json = await response.Content.ReadAsStringAsync();

                _cache.Set(cacheKey, json, TimeSpan.FromSeconds(45));

                return json;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("CoinGecko API request timed out.");
                return "Los datos de mercado no están disponibles " +
                       "(timeout de conexión).";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CoinGecko data.");
                return "Los datos de mercado no están disponibles " +
                       "en este momento.";
            }
        }

        private static string BuildCoinGeckoUrl(string ids)
        {
            var sb = new StringBuilder(COINGECKO_BASE_URL);
            sb.Append("/coins/markets?vs_currency=usd");

            if (!string.IsNullOrEmpty(ids))
            {
                sb.Append("&ids=");
                sb.Append(Uri.EscapeDataString(ids));
            }
            else
            {
                sb.Append("&order=market_cap_desc&per_page=10");
            }

            sb.Append("&sparkline=false&price_change_percentage=24h");

            return sb.ToString();
        }
    }
}