namespace ClusterKit.Log.ElasticSearch
{
    using System;
    using System.Linq;

    using Akka.Configuration;

    using ClusterKit.Core;

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
        /// Perfoms configuration
        /// </summary>
        /// <param name="configuration">Previous configuration</param>
        /// <param name="config">Akka configuration</param>
        /// <returns>Updated configuration</returns>
        public LoggerConfiguration Configure(LoggerConfiguration configuration, Config config)
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
                "{Type}: \n\tMinimum level: {MinimumLevel}\n\tIndex format: {IndexFormat}\n\tNodes:\n\t\t{NodeList}\n",
                this.GetType().FullName,
                minimumLevel,
                indexFormat,
                string.Join("\n\t\t", nodes));


            SelfLog.Enable(Console.WriteLine);
            var options = new ElasticsearchSinkOptions(nodes.Select(s => new Uri(s)))
                              {
                                  MinimumLogEventLevel = level,
                                  AutoRegisterTemplate = true,
                                  IndexFormat = indexFormat,
                                  //CustomFormatter = new ExceptionAsJsonObjectFormatter(renderMessage: true)

                              };

            return configuration.WriteTo.Elasticsearch(options);


        }
    }
}
