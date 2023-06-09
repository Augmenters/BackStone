using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
    public sealed class YelpCoordinate
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}

