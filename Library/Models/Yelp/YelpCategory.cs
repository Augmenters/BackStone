using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
    public sealed class YelpCategory
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}

