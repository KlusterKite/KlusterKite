// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogAccessRule.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The action description to log a field / method access
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using ClusterKit.API.Client.Attributes.Authorization;
    using ClusterKit.Security.Client;

    /// <summary>
    /// The action description to log a field / method access
    /// </summary>
    public class LogAccessRule
    {
        /// <summary>
        /// Gets or sets the severity
        /// </summary>
        public EnSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the log type.
        /// </summary>
        /// <remarks>
        /// If absent, the most suitable type will be applied automatically
        /// </remarks>
        public SecurityLog.EnType? Type { get; set; }

        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        /// <remarks>
        /// If absent, the api field path will be logged
        /// </remarks>
        public string LogMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of connection actions to apply attribute to
        /// </summary>
        /// <returns>
        /// Usually data modification operation a logged by themselves at operation place
        /// </returns>
        public EnConnectionAction ConnectionActions { get; set; } = EnConnectionAction.All;
    }
}
