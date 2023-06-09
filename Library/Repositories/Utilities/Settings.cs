using System;
using Microsoft.Extensions.Configuration;

namespace Library.Repositories.Utilities
{
    public class Settings : ISettings
    {
        public string YelpApiBaseUrl { get; private set; }
        public string YelpGraphQLUrl { get; private set; }

        public string YelpApiKey { get; private set; }
        public long CacheSize { get; private set; }
        public string GridPrecision { get; private set; }

        public Settings(IConfiguration configuration)
        {
            YelpApiBaseUrl = configuration.GetSection("ServiceEndpoints")["YelpApiBaseUrl"];
            YelpGraphQLUrl = configuration.GetSection("ServiceEndpoints")["YelpGraphQLUrl"];
            CacheSize = long.Parse(configuration.GetSection("AppSettings")["CacheSize"]);

            YelpApiKey = Environment.GetEnvironmentVariable("YelpApiKey");

            if (string.IsNullOrWhiteSpace(YelpApiKey))
                throw new Exception("Yelp API key is not set");
        }
    }
}

