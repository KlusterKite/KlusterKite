// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfigurator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Logger configuration, that enriches logs with template name
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client
{
    using Akka.Configuration;

    using JetBrains.Annotations;

    using KlusterKite.Core.Log;

    using Serilog;

    /// <summary>
    /// Logger configuration, that enriches logs with template name
    /// </summary>
    [UsedImplicitly]
    public class LoggerConfigurator : ILoggerConfigurator
    {
        /// <summary>
        /// Performs configuration
        /// </summary>
        /// <param name="configuration">Previous configuration</param>
        /// <param name="config">Akka configuration</param>
        /// <returns>Updated configuration</returns>
        public LoggerConfiguration Configure(LoggerConfiguration configuration, Config config)
        {
            var templateName = config.GetString("KlusterKite.NodeManager.NodeTemplate");
            return string.IsNullOrWhiteSpace(templateName)
                       ? configuration
                       : configuration.Enrich.WithProperty("nodeTemplate", templateName);
        }
    }
}
