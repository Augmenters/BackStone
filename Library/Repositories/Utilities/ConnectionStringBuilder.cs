using System;
using Microsoft.Extensions.Configuration;

namespace Library.Repositories.Utilities
{
	public static class ConnectionStringBuilder
	{
        public static string Build(IConfiguration configuration)
        {
            var host = configuration.GetSection("AppSettings")["PostGresHost"];
            var port = configuration.GetSection("AppSettings")["PostGresPort"];
            var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
            var catalog = Environment.GetEnvironmentVariable("POSTGRES_DB");

            return $"Server={host}; Port={port}; User ID={user}; Password={password}; Database={catalog};";
        }
    }
}

