// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILoggerConfigurator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The logger configurator
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core.Log
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