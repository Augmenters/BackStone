using System;
using Library.Repositories.Utilities;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests
{
	public class TestSettings : ISettings
	{
        private readonly IConfigurationRoot config;

        public TestSettings()
        {
            config = new ConfigurationManager().AddJsonFile("appsettings.json").Build();

            if (config == null)
                throw new FileNotFoundException();
        }

        public string YelpApiBaseUrl => config.GetSection("ServiceEndpoints")["YelpApiBaseUrl"];

        public string YelpGraphQLUrl => config.GetSection("ServiceEndpoints")["YelpGraphQLUrl"];

        public string YelpApiKey => Environment.GetEnvironmentVariable("YelpApiKey");

        public long CacheSize => throw new NotImplementedException();

        public string GridPrecision => throw new NotImplementedException();
    }
}

