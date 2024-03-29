﻿using System;
using Newtonsoft.Json;

namespace Library.Models.Yelp
{
    public sealed class User
	{
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
	}
}

