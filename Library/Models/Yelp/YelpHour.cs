using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
    public class YelpHours
    {
        [JsonProperty("open")]
        public YelpHour[] Open { get; set; }
    }

    public class YelpHour
	{
        [JsonProperty("start")]
        public int Open { get; set; }

        [JsonProperty("end")]
        public int Close { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }
	}
}

