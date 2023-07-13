using System;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Serilog.Sinks.Elasticsearch;
using Microsoft.Extensions.Logging;
using Serilog;
using SeriLogger = Serilog.Log;

namespace Library.Repositories.Utilities
{
    public static class LoggingProvider
    {
        public static void LogFatal(this Exception ex, string? message)
        {
            SeriLogger.Fatal(ex, message ?? ex.Message);
        }

        public static void Log(this Exception ex, string? message)
        {
            SeriLogger.Error(ex, message ?? ex.Message);
        }

        public static void LogInfo(string message)
        {
            SeriLogger.Information(message);
        }

        public static void ConfigureLogging(this ILoggingBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .AddJsonFile(
                                        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                                        optional: true)
                                    .Build();

            SeriLogger.Logger = new LoggerConfiguration()
                                    .Enrich.FromLogContext()
                                    .Enrich.WithMachineName()
                                    .WriteTo.Debug()
                                    .WriteTo.Console()
                                    .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                                    .Enrich.WithProperty("Environment", environment)
                                    .ReadFrom.Configuration(configuration)
                                    .CreateLogger();

            builder.AddSerilog(SeriLogger.Logger);
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
            };
        }
    }
}

