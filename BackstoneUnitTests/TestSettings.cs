using System;
using Library.Repositories.Utilities;
using Microsoft.Extensions.Configuration;

namespace UnitTests
{
    public class TestSettings : ISettings
    {
        public string YelpApiBaseUrl => throw new NotImplementedException();

        public string YelpGraphQLUrl => throw new NotImplementedException();

        public string YelpApiKey => throw new NotImplementedException();

        public long CacheSize => throw new NotImplementedException();

        public double SearchRadius => throw new NotImplementedException();

        int ISettings.GridPrecision => 7;
    }
}

