using System;
namespace Library.Repositories.Utilities.Interfaces
{
    public interface ISettings
    {
        //ServiceEndpoints
        public string YelpApiBaseUrl { get; }
        public string YelpGraphQLUrl { get; }

        //Keys
        public string YelpApiKey { get; }

        //AppSettings
        public long CacheSize { get; }
        public int GridPrecision { get; }
        public double SearchRadius { get; }
    }
}

