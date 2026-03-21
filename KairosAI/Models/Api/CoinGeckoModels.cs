using System.Text.Json.Serialization;

namespace KairosAI.Models.Api
{
    // Respuesta de /coins/markets
    public class CoinGeckoMarketItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("image")]
        public string Image { get; set; } = "";

        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonPropertyName("market_cap")]
        public long MarketCap { get; set; }

        [JsonPropertyName("market_cap_rank")]
        public int? MarketCapRank { get; set; }

        [JsonPropertyName("price_change_percentage_24h")]
        public decimal? PriceChangePercentage24h { get; set; }

        [JsonPropertyName("price_change_24h")]
        public decimal? PriceChange24h { get; set; }

        [JsonPropertyName("high_24h")]
        public decimal? High24h { get; set; }

        [JsonPropertyName("low_24h")]
        public decimal? Low24h { get; set; }

        [JsonPropertyName("total_volume")]
        public long? TotalVolume { get; set; }

        [JsonPropertyName("sparkline_in_7d")]
        public SparklineData? SparklineIn7d { get; set; }
    }

    public class SparklineData
    {
        [JsonPropertyName("price")]
        public List<decimal>? Price { get; set; }
    }

    // Respuesta de /search/trending
    public class CoinGeckoTrendingResponse
    {
        [JsonPropertyName("coins")]
        public List<TrendingCoinWrapper>? Coins { get; set; }
    }

    public class TrendingCoinWrapper
    {
        [JsonPropertyName("item")]
        public TrendingCoinItem? Item { get; set; }
    }

    public class TrendingCoinItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("coin_id")]
        public int CoinId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = "";

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; } = "";

        [JsonPropertyName("data")]
        public TrendingCoinData? Data { get; set; }
    }

    public class TrendingCoinData
    {
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("price_change_percentage_24h")]
        public Dictionary<string, decimal>? PriceChangePercentage24h { get; set; }
    }
}