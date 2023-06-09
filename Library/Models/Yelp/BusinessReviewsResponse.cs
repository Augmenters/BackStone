using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
	public sealed class BusinessReviewsResponse
	{
        [JsonProperty("reviews")]
        public IEnumerable<YelpReview> Reviews { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("possible_languages")]
        public string[] PossibleLanguages { get; set; }
	}
}

