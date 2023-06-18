using System;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
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

        public double SearchRadius => double.Parse(config.GetSection("AppSettings")["SearchRadius"]);

        public long CacheSize => throw new NotImplementedException();

        public int GridPrecision => throw new NotImplementedException();
    }
}

