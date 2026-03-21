using KairosAI.Models.Api;
using KairosAI.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace KairosAI.Services
{
    public class NewsAggregatorService : INewsAggregatorService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IStockService _stockService;
        private readonly ILogger<NewsAggregatorService> _logger;

        // Mapeo de keywords a IDs de CoinGecko
        private static readonly Dictionary<string, string> CryptoKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bitcoin"] = "bitcoin",
            ["btc"] = "bitcoin",
            ["ethereum"] = "ethereum",
            ["eth"] = "ethereum",
            ["solana"] = "solana",
            ["sol"] = "solana",
            ["cardano"] = "cardano",
            ["ada"] = "cardano",
            ["xrp"] = "ripple",
            ["ripple"] = "ripple",
            ["dogecoin"] = "dogecoin",
            ["doge"] = "dogecoin",
            ["bnb"] = "binancecoin",
            ["binance"] = "binancecoin",
            ["polygon"] = "matic-network",
            ["matic"] = "matic-network",
            ["avalanche"] = "avalanche-2",
            ["avax"] = "avalanche-2",
            ["polkadot"] = "polkadot",
            ["dot"] = "polkadot",
            ["chainlink"] = "chainlink",
            ["link"] = "chainlink",
            ["litecoin"] = "litecoin",
            ["ltc"] = "litecoin",
            ["crypto"] = "bitcoin",
            ["cripto"] = "bitcoin",
            ["criptomoneda"] = "bitcoin",
            ["defi"] = "ethereum",
        };

        // Mapeo de keywords a tickers de bolsa
        private static readonly Dictionary<string, string> StockKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["tesla"] = "TSLA",
            ["tsla"] = "TSLA",
            ["apple"] = "AAPL",
            ["aapl"] = "AAPL",
            ["microsoft"] = "MSFT",
            ["msft"] = "MSFT",
            ["amazon"] = "AMZN",
            ["amzn"] = "AMZN",
            ["google"] = "GOOGL",
            ["alphabet"] = "GOOGL",
            ["meta"] = "META",
            ["facebook"] = "META",
            ["nvidia"] = "NVDA",
            ["nvda"] = "NVDA",
            ["s&p"] = "SPY",
            ["s&p 500"] = "SPY",
            ["spy"] = "SPY",
            ["nasdaq"] = "QQQ",
            ["qqq"] = "QQQ",
            ["dow"] = "DIA",
            ["dow jones"] = "DIA",
            ["bolsa"] = "SPY",
        };

        public NewsAggregatorService(
            ICryptoService cryptoService,
            IStockService stockService,
            ILogger<NewsAggregatorService> logger)
        {
            _cryptoService = cryptoService;
            _stockService = stockService;
            _logger = logger;
        }

        public async Task<NewsIndexViewModel> GetAggregatedNewsAsync()
        {
            var model = new NewsIndexViewModel();

            try
            {
                // 1. Obtener noticias y precios en paralelo
                var newsTask = _stockService.GetNewsAsync(
                    search: "finance crypto cryptocurrency stock market",
                    limit: 30,
                    language: "es"
                );

                // También buscar en inglés para más resultados
                var newsEnTask = _stockService.GetNewsAsync(
                    search: "bitcoin ethereum stock market crypto",
                    limit: 15,
                    language: "en"
                );

                var cryptoTask = _cryptoService.GetMarketDataAsync(
                    vsCurrency: "usd",
                    perPage: 20
                );

                var stockTickers = new[] { "AAPL", "TSLA", "MSFT", "NVDA", "AMZN", "GOOGL", "META", "SPY", "QQQ" };
                var stocksTask = _stockService.GetQuotesAsync(stockTickers);

                // Esperar todas las tareas
                await Task.WhenAll(newsTask, newsEnTask, cryptoTask, stocksTask);

                var newsEs = await newsTask;
                var newsEn = await newsEnTask;
                var cryptoData = await cryptoTask;
                var stockData = await stocksTask;

                // 2. Construir diccionarios de precios para lookup rápido
                var cryptoPrices = cryptoData.ToDictionary(
                    c => c.Id,
                    c => c,
                    StringComparer.OrdinalIgnoreCase
                );

                var stockPrices = stockData.ToDictionary(
                    s => s.Ticker,
                    s => s,
                    StringComparer.OrdinalIgnoreCase
                );

                // 3. Combinar noticias (español primero)
                var allNews = new List<StockDataNewsItem>();
                allNews.AddRange(newsEs);
                allNews.AddRange(newsEn);

                // Deduplicar por título similar
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var uniqueNews = new List<StockDataNewsItem>();
                foreach (var n in allNews)
                {
                    var key = n.Title.Length > 30 ? n.Title[..30] : n.Title;
                    if (seen.Add(key))
                        uniqueNews.Add(n);
                }

                // 4. Transformar a NewsItem con precios asociados
                int idCounter = 1;
                foreach (var raw in uniqueNews.Take(25))
                {
                    var newsItem = new NewsItem
                    {
                        Id = idCounter++,
                        Title = raw.Title,
                        Source = raw.Source ?? "Financial News",
                        Summary = !string.IsNullOrEmpty(raw.Description)
                            ? TruncateText(raw.Description, 200)
                            : (raw.Snippet ?? ""),
                        Url = raw.Url,
                        ImageUrl = raw.ImageUrl,
                        PublishedAt = raw.PublishedAt ?? DateTime.UtcNow,
                        TimeAgo = GetTimeAgo(raw.PublishedAt ?? DateTime.UtcNow),
                        Sentiment = DetermineSentiment(raw),
                        Category = DetectCategory(raw),
                        RelatedAssets = new List<RelatedAssetPrice>()
                    };

                    // 5. Asociar precios de activos mencionados
                    string fullText = $"{raw.Title} {raw.Description} {raw.Keywords}".ToLower();

                    // Buscar criptos mencionadas
                    foreach (var kvp in CryptoKeywords)
                    {
                        if (fullText.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (cryptoPrices.TryGetValue(kvp.Value, out var coin))
                            {
                                // Evitar duplicados
                                if (!newsItem.RelatedAssets.Any(a => a.Symbol.Equals(coin.Symbol, StringComparison.OrdinalIgnoreCase)))
                                {
                                    newsItem.RelatedAssets.Add(new RelatedAssetPrice
                                    {
                                        Symbol = coin.Symbol.ToUpper(),
                                        Name = coin.Name,
                                        Price = coin.CurrentPrice,
                                        Change24h = coin.PriceChangePercentage24h ?? 0,
                                        AssetType = "crypto",
                                        ImageUrl = coin.Image,
                                        High24h = coin.High24h ?? 0,
                                        Low24h = coin.Low24h ?? 0,
                                        Volume = coin.TotalVolume ?? 0,
                                        SparklineData = coin.SparklineIn7d?.Price?
                                            .TakeLast(24)
                                            .Select(p => (double)p)
                                            .ToList() ?? new List<double>()
                                    });
                                }
                            }
                        }
                    }

                    // Buscar acciones mencionadas
                    foreach (var kvp in StockKeywords)
                    {
                        if (fullText.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (stockPrices.TryGetValue(kvp.Value, out var stock))
                            {
                                if (!newsItem.RelatedAssets.Any(a => a.Symbol.Equals(stock.Ticker, StringComparison.OrdinalIgnoreCase)))
                                {
                                    newsItem.RelatedAssets.Add(new RelatedAssetPrice
                                    {
                                        Symbol = stock.Ticker,
                                        Name = stock.Name,
                                        Price = stock.Price ?? 0,
                                        Change24h = (decimal)(stock.ChangePercent ?? 0),
                                        AssetType = "stock",
                                        High24h = stock.DayHigh ?? 0,
                                        Low24h = stock.DayLow ?? 0,
                                        Volume = stock.Volume ?? 0,
                                        IsMarketOpen = stock.IsMarketOpen ?? false
                                    });
                                }
                            }
                        }
                    }

                    // También revisar entities de StockData
                    if (raw.Entities != null)
                    {
                        foreach (var entity in raw.Entities.Take(3))
                        {
                            if (!string.IsNullOrEmpty(entity.Symbol) &&
                                !newsItem.RelatedAssets.Any(a => a.Symbol.Equals(entity.Symbol, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (stockPrices.TryGetValue(entity.Symbol, out var stock))
                                {
                                    newsItem.RelatedAssets.Add(new RelatedAssetPrice
                                    {
                                        Symbol = stock.Ticker,
                                        Name = entity.Name ?? stock.Name,
                                        Price = stock.Price ?? 0,
                                        Change24h = (decimal)(stock.ChangePercent ?? 0),
                                        AssetType = "stock",
                                        High24h = stock.DayHigh ?? 0,
                                        Low24h = stock.DayLow ?? 0,
                                        Volume = stock.Volume ?? 0,
                                        IsMarketOpen = stock.IsMarketOpen ?? false
                                    });
                                }
                            }
                        }
                    }

                    // Limitar a 3 activos por noticia
                    newsItem.RelatedAssets = newsItem.RelatedAssets.Take(3).ToList();

                    model.Headlines.Add(newsItem);
                }

                // 6. Agregar ticker de precios principales al modelo
                model.MarketTicker = cryptoData.Take(8).Select(c => new MarketTickerItem
                {
                    Symbol = c.Symbol.ToUpper(),
                    Name = c.Name,
                    Price = c.CurrentPrice,
                    Change24h = c.PriceChangePercentage24h ?? 0,
                    ImageUrl = c.Image,
                    AssetType = "crypto"
                }).ToList();

                // Agregar algunos índices al ticker
                foreach (var stock in stockData.Where(s => new[] { "SPY", "QQQ", "AAPL", "TSLA" }.Contains(s.Ticker)))
                {
                    model.MarketTicker.Add(new MarketTickerItem
                    {
                        Symbol = stock.Ticker,
                        Name = stock.Name,
                        Price = stock.Price ?? 0,
                        Change24h = (decimal)(stock.ChangePercent ?? 0),
                        AssetType = "stock"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar noticias");

                // Fallback: devolver datos de ejemplo si las APIs fallan
                model.Headlines = GetFallbackNews();
            }

            return model;
        }

        /// <summary>
        /// Determina el sentimiento basado en las entidades de StockData 
        /// o análisis básico de keywords
        /// </summary>
        private string DetermineSentiment(StockDataNewsItem raw)
        {
            // 1. Usar sentiment score de las entidades si está disponible
            if (raw.Entities?.Any() == true)
            {
                var avgSentiment = raw.Entities
                    .Where(e => e.SentimentScore.HasValue)
                    .Select(e => e.SentimentScore!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                if (avgSentiment > 0.15m) return "Positivo";
                if (avgSentiment < -0.15m) return "Negativo";
                return "Neutro";
            }

            // 2. Fallback: análisis básico de keywords
            string text = $"{raw.Title} {raw.Description}".ToLower();

            var positiveWords = new[] {
                "sube", "alcista", "rally", "máximo", "gana", "crece", "bull",
                "bullish", "surge", "soars", "gains", "high", "record",
                "optimismo", "positivo", "recupera", "impulsa", "supera"
            };

            var negativeWords = new[] {
                "baja", "bajista", "cae", "desploma", "pierde", "crash",
                "bear", "bearish", "falls", "drops", "plunge", "decline",
                "crisis", "riesgo", "preocupa", "mínimo", "colapso",
                "regulación", "prohibición", "sanción"
            };

            int posScore = positiveWords.Count(w => text.Contains(w));
            int negScore = negativeWords.Count(w => text.Contains(w));

            if (posScore > negScore) return "Positivo";
            if (negScore > posScore) return "Negativo";
            return "Neutro";
        }

        private string DetectCategory(StockDataNewsItem raw)
        {
            string text = $"{raw.Title} {raw.Description} {raw.Keywords}".ToLower();

            if (CryptoKeywords.Keys.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "cripto";

            if (text.Contains("forex") || text.Contains("dólar") || text.Contains("dollar") ||
                text.Contains("euro") || text.Contains("yen") || text.Contains("divisa") ||
                text.Contains("currency") || text.Contains("exchange rate"))
                return "forex";

            if (text.Contains("bolsa") || text.Contains("stock") || text.Contains("acción") ||
                text.Contains("nasdaq") || text.Contains("s&p") || text.Contains("dow") ||
                text.Contains("shares") || text.Contains("equity"))
                return "acciones";

            if (text.Contains("fed") || text.Contains("inflación") || text.Contains("inflation") ||
                text.Contains("pib") || text.Contains("gdp") || text.Contains("macro") ||
                text.Contains("interest rate") || text.Contains("tasa de interés") ||
                text.Contains("banco central") || text.Contains("central bank"))
                return "macro";

            return "default";
        }

        private string GetTimeAgo(DateTime publishedAt)
        {
            var diff = DateTime.UtcNow - publishedAt;

            if (diff.TotalMinutes < 1) return "Ahora";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes}min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays}d";
            return publishedAt.ToString("dd MMM");
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length <= maxLength) return text;

            // Cortar en el último espacio antes del límite
            int lastSpace = text.LastIndexOf(' ', maxLength);
            return (lastSpace > 0 ? text[..lastSpace] : text[..maxLength]) + "…";
        }

        /// <summary>
        /// Noticias de fallback si las APIs no responden
        /// </summary>
        private List<NewsItem> GetFallbackNews()
        {
            return new List<NewsItem>
            {
                new()
                {
                    Id = 1,
                    Title = "Bitcoin se mantiene estable mientras los mercados esperan datos de la FED",
                    Source = "KairosAI",
                    Summary = "El precio de Bitcoin se mantiene en rango mientras los inversores esperan la decisión de tasas de interés de la Reserva Federal.",
                    TimeAgo = "Hace 1h",
                    Sentiment = "Neutro",
                    Category = "cripto",
                    Url = "#",
                    RelatedAssets = new List<RelatedAssetPrice>
                    {
                        new() { Symbol = "BTC", Name = "Bitcoin", Price = 0, Change24h = 0, AssetType = "crypto" }
                    }
                },
                new()
                {
                    Id = 2,
                    Title = "El S&P 500 alcanza nuevos máximos históricos",
                    Source = "KairosAI",
                    Summary = "Los índices bursátiles de EE.UU. continúan su tendencia alcista impulsados por el sector tecnológico.",
                    TimeAgo = "Hace 3h",
                    Sentiment = "Positivo",
                    Category = "acciones",
                    Url = "#",
                    RelatedAssets = new List<RelatedAssetPrice>
                    {
                        new() { Symbol = "SPY", Name = "S&P 500 ETF", Price = 0, Change24h = 0, AssetType = "stock" }
                    }
                }
            };
        }
    }
}