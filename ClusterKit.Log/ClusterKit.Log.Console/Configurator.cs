// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Colored consoled configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Log.Console
{
    using System;

    using Akka.Configuration;

    using ClusterKit.Core.Log;

    using JetBrains.Annotations;

    using Serilog;
    using Serilog.Events;

    /// <summary>
    /// Colored consoled configuration
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
            var minimumLevel = config.GetString("ClusterKit.Log.Console.minimumLevel", "none")?.Trim();

            LogEventLevel level;
            if (!Enum.TryParse(minimumLevel, true, out level))
            {
                return configuration;
            }

            return configuration.WriteTo.ColoredConsole(LogEventLevel.Debug);
        }
    }
}
