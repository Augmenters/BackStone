using System;
using Library.Repositories.Utilities.Interfaces;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Utilities;

namespace Library.Repositories.Utilities
{
    public class Settings : ISettings
    {
        public string YelpApiBaseUrl { get; private set; }
        public string YelpGraphQLUrl { get; private set; }

        public string YelpApiKey { get; private set; }
        public long CacheSize { get; private set; }
        public double SearchRadius { get; private set; }
        public int GridPrecision { get; private set; }

        public Settings(IConfiguration configuration)
        {
            YelpApiBaseUrl = configuration.GetSection("ServiceEndpoints")["YelpApiBaseUrl"];
            YelpGraphQLUrl = configuration.GetSection("ServiceEndpoints")["YelpGraphQLUrl"];
            CacheSize = long.Parse(configuration.GetSection("AppSettings")["CacheSize"]);
            SearchRadius = double.Parse(configuration.GetSection("AppSettings")["SearchRadius"]);
            GridPrecision = int.Parse(configuration.GetSection("AppSettings")["GridPrecision"]);

            YelpApiKey = Environment.GetEnvironmentVariable("YelpApiKey");

            if (string.IsNullOrWhiteSpace(YelpApiKey))
            {
                var ex = new Exception("Yelp API key is not set");
                ex.LogFatal("Failed to initialize settings");
                throw ex;
            }
        }
    }
}

