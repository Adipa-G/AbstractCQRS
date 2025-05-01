using System;
using System.IO;

using Microsoft.Extensions.Configuration;

using Serilog.Events;

namespace AbstractCqrs.Sample.Service
{
    public class Config
    {
        private const string ConnectionStringKey = "ConnectionString";
        private const string LogFilePathKey = "LogFilePath";
        private const string LogLevelKey = "LogLevel";

        private static IConfigurationRoot configuration;

        public static string ConnectionString
        {
            get
            {
                Init();
                return configuration[ConnectionStringKey];
            }
        }

        public static string LogFilePath
        {
            get
            {
                Init();
                return configuration[LogFilePathKey];
            }
        }

        public static LogEventLevel LogLevel
        {
            get
            {
                Init();
                return (LogEventLevel)Enum.Parse(typeof(LogEventLevel),
                    configuration[LogLevelKey]);
            }
        }

        private static void Init()
        {
            if (configuration == null)
                configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
        }
    }
}