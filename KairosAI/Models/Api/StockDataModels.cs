using System.Text.Json.Serialization;

namespace KairosAI.Models.Api
{
    // Respuesta de StockData.org /v1/data/quote
    public class StockDataResponse
    {
        [JsonPropertyName("meta")]
        public StockDataMeta? Meta { get; set; }

        [JsonPropertyName("data")]
        public List<StockQuote>? Data { get; set; }
    }

    public class StockDataMeta
    {
        [JsonPropertyName("requested")]
        public int Requested { get; set; }

        [JsonPropertyName("returned")]
        public int Returned { get; set; }
    }

    public class StockQuote
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("exchange_short")]
        public string ExchangeShort { get; set; } = "";

        [JsonPropertyName("exchange_long")]
        public string ExchangeLong { get; set; } = "";

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("day_high")]
        public decimal? DayHigh { get; set; }

        [JsonPropertyName("day_low")]
        public decimal? DayLow { get; set; }

        [JsonPropertyName("day_open")]
        public decimal? DayOpen { get; set; }

        [JsonPropertyName("previous_close_price")]
        public decimal? PreviousClosePrice { get; set; }

        [JsonPropertyName("day_change")]
        public decimal? DayChange { get; set; }

        [JsonPropertyName("change_percent")]
        public decimal? ChangePercent { get; set; }

        [JsonPropertyName("volume")]
        public long? Volume { get; set; }

        [JsonPropertyName("is_market_open")]
        public bool? IsMarketOpen { get; set; }

        [JsonPropertyName("52_week_high")]
        public decimal? Week52High { get; set; }

        [JsonPropertyName("52_week_low")]
        public decimal? Week52Low { get; set; }
    }

    // Respuesta de StockData.org /v1/news/all
    public class StockDataNewsResponse
    {
        [JsonPropertyName("meta")]
        public StockDataNewsMeta? Meta { get; set; }

        [JsonPropertyName("data")]
        public List<StockDataNewsItem>? Data { get; set; }
    }

    public class StockDataNewsMeta
    {
        [JsonPropertyName("found")]
        public int Found { get; set; }

        [JsonPropertyName("returned")]
        public int Returned { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }
    }

    public class StockDataNewsItem
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("keywords")]
        public string? Keywords { get; set; }

        [JsonPropertyName("snippet")]
        public string? Snippet { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("entities")]
        public List<StockDataEntity>? Entities { get; set; }

        [JsonPropertyName("similar")]
        public List<StockDataNewsItem>? Similar { get; set; }
    }

    public class StockDataEntity
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("exchange_long")]
        public string? ExchangeLong { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("industry")]
        public string? Industry { get; set; }

        [JsonPropertyName("match_score")]
        public decimal? MatchScore { get; set; }

        [JsonPropertyName("sentiment_score")]
        public decimal? SentimentScore { get; set; }

        [JsonPropertyName("highlights")]
        public List<StockDataHighlight>? Highlights { get; set; }
    }

    public class StockDataHighlight
    {
        [JsonPropertyName("highlight")]
        public string? Highlight { get; set; }

        [JsonPropertyName("sentiment")]
        public decimal? Sentiment { get; set; }

        [JsonPropertyName("highlighted_in")]
        public string? HighlightedIn { get; set; }
    }
}