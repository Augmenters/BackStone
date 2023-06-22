using System;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
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

        public int GridPrecision => 7;
    }
}

