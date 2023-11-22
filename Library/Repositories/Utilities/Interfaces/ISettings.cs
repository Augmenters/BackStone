using System;
namespace Library.Repositories.Utilities.Interfaces
{
    public interface ISettings
    {
        //Connection Strings
        public string BackstoneDB { get; }

        //Service Endpoints
        public string YelpApiBaseUrl { get; }
        public string YelpGraphQLUrl { get; }

        //Keys
        public string YelpApiKey { get; }

        //App Settings
        public long CacheSize { get; }
        public int GridPrecision { get; }
        public double SearchRadius { get; }
        public int Limit { get; }
    }
}

