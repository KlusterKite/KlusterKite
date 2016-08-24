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
        /// Perfoms configuration
        /// </summary>
        /// <param name="configuration">Previous configuration</param>
        /// <param name="config">Akka configuration</param>
        /// <returns>Updated configuration</returns>
        LoggerConfiguration Configure(LoggerConfiguration configuration, Config config);
    }
}