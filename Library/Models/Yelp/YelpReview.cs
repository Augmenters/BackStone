using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
    public sealed class YelpReview
	{
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("time_created")]
        public string TimeCreated { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
	}
}

