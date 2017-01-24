// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILoggerConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The logger configurator
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using Akka.Configuration;

    using Serilog;

    /// <summary>
    /// The logger configurator
    /// </summary>
    public interface ILoggerConfigurator
    {
        /// <summary>
        /// Performs configuration
        /// </summary>
        /// <param name="configuration">Previous configuration</param>
        /// <param name="config">Akka configuration</param>
        /// <returns>Updated configuration</returns>
        LoggerConfiguration Configure(LoggerConfiguration configuration, Config config);
    }
}