// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Elastic search configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Log.ElasticSearch
{
    using System;
    using System.Linq;

    using Akka.Configuration;

    using ClusterKit.Core.Log;

    using JetBrains.Annotations;

    using Serilog;
    using Serilog.Debugging;
    using Serilog.Events;
    using Serilog.Sinks.Elasticsearch;

    /// <summary>
    /// Elastic search configuration
    /// </summary>
    [UsedImplicitly]
    public class Configurator : ILoggerConfigurator
    {
        /// <summary>
        /// Performs configuration
        /// </summary>
        /// <param name="configuration">Previous configuration</param>
        /// <param name="config">Akka configuration</param>
        /// <returns>Updated configuration</returns>
        public LoggerConfiguration Configure(LoggerConfiguration configuration, Config config)
        {
            SelfLog.Enable(Console.WriteLine);
            var loggerConfiguration = configuration;
            loggerConfiguration = this.ConfigureStandardLog(loggerConfiguration, config);
            loggerConfiguration = this.ConfigureSecurityLog(loggerConfiguration, config);
            return loggerConfiguration;
        }

        /// <summary>
        /// Configures standard log flow
        /// </summary>
        /// <param name="configuration">The initial log configuration</param>
        /// <param name="config">The service config</param>
        /// <returns>The updated log configuration</returns>
        private LoggerConfiguration ConfigureStandardLog(LoggerConfiguration configuration, Config config)
        {
            var minimumLevel = config.GetString("ClusterKit.Log.ElasticSearch.minimumLevel", "none")?.Trim();

            LogEventLevel level;
            if (!Enum.TryParse(minimumLevel, true, out level))
            {
                return configuration;
            }

            var nodes = config.GetStringList("ClusterKit.Log.ElasticSearch.nodes");

            var indexFormat = config.GetString("ClusterKit.Log.ElasticSearch.indexFormat", "logstash-{0:yyyy.MM.dd}");

            Log.Information(
                "{Type} Standard log: \n\tMinimum level: {MinimumLevel}\n\tIndex format: {IndexFormat}\n\tNodes:\n\t\t{NodeList}\n",
                this.GetType().FullName,
                minimumLevel,
                indexFormat,
                string.Join("\n\t\t", nodes));

            var options = new ElasticsearchSinkOptions(nodes.Select(s => new Uri(s)))
                              {
                                  MinimumLogEventLevel = level,
                                  AutoRegisterTemplate = true,
                                  IndexFormat = indexFormat,
                              };

            Func<LogEvent, bool> logFilter = log =>
                {
                    LogEventPropertyValue value;
                    return !log.Properties.TryGetValue(Constants.LogRecordTypeKey, out value)
                           || value.ToString() == EnLogRecordType.Default.ToString();
                };

            return
                configuration.WriteTo.Logger(
                    c => c.Filter.ByIncludingOnly(logFilter).WriteTo.Elasticsearch(options));
        }

        /// <summary>
        /// Configures standard log flow
        /// </summary>
        /// <param name="configuration">The initial log configuration</param>
        /// <param name="config">The service config</param>
        /// <returns>The updated log configuration</returns>
        private LoggerConfiguration ConfigureSecurityLog(LoggerConfiguration configuration, Config config)
        {
            var minimumLevel = config.GetString("ClusterKit.Log.ElasticSearch.securityMinimumLevel", "none")?.Trim();

            LogEventLevel level;
            if (!Enum.TryParse(minimumLevel, true, out level))
            {
                return configuration;
            }

            var nodes = config.GetStringList("ClusterKit.Log.ElasticSearch.securityNodes");

            var indexFormat = config.GetString("ClusterKit.Log.ElasticSearch.securityIndexFormat", "security-{0:yyyy.MM.dd}");

            Log.Information(
                "{Type} Security log: \n\tMinimum level: {MinimumLevel}\n\tIndex format: {IndexFormat}\n\tNodes:\n\t\t{NodeList}\n",
                this.GetType().FullName,
                minimumLevel,
                indexFormat,
                string.Join("\n\t\t", nodes));

            var options = new ElasticsearchSinkOptions(nodes.Select(s => new Uri(s)))
            {
                MinimumLogEventLevel = level,
                AutoRegisterTemplate = true,
                IndexFormat = indexFormat,
            };

            Func<LogEvent, bool> logFilter = log =>
                {
                    LogEventPropertyValue value;
                     return log.Properties.TryGetValue(Constants.LogRecordTypeKey, out value)
                                     && (value as ScalarValue)?.Value is EnLogRecordType
                                     && (EnLogRecordType)((ScalarValue)value).Value == EnLogRecordType.Security;
                };

            return
                configuration.WriteTo.Logger(c => c.Filter.ByIncludingOnly(logFilter).WriteTo.Elasticsearch(options));
        }
    }
}
