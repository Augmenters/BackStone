using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
    public sealed class YelpBusiness
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("is_closed")]
        public bool IsClosed { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("review_count")]
        public int ReviewCount { get; set; }

        [JsonProperty("categories")]
        public YelpCategory[] Categories { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("coordinates")]
        public YelpCoordinate Coordinate { get; set; }

        [JsonProperty("transactions")]
        public string[] Transactions { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("location")]
        public YelpAddress Location { get; set; }

        [JsonProperty("display_phone")]
        public string DisplayPhone { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("hours")]
        public YelpHours[] Hours { get; set; }
    }
}

