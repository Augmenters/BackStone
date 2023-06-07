using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
	public sealed class BusinessQueryResponse
	{
        [JsonProperty("search")]
        public BusinessQueryResponseData? Search { get; set; }
    }

    public sealed class BusinessQueryResponseData
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("business")]
        public IEnumerable<YelpBusiness> Businesses { get; set; }
    }
}

