// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogAccessAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the LogAccessAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes.Authorization
{
    using System;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The access to the marked field will be logged to security log with specified severity
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LogAccessAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the severity
        /// </summary>
        public EnSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        public string LogMessage { get; set; }
    }
}
