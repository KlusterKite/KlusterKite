// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogAccessAttribute.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the LogAccessAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes.Authorization
{
    using System;
    using System.Reflection;

    using KlusterKite.Security.Attributes;

    /// <summary>
    /// The access to the marked field will be logged to security log with specified severity
    /// </summary>
    /// <remarks>
    /// In case of multiple attributes applied to the same case the one with the highest severity will be used
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LogAccessAttribute : Attribute
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
        public EnSecurityLogType? Type { get; set; }

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

        /// <summary>
        /// Creates <see cref="LogAccessRule"/> from this attribute
        /// </summary>
        /// <param name="memberInfo">
        /// The member Info.
        /// </param>
        /// <returns>
        /// The <see cref="LogAccessRule"/>
        /// </returns>
        public LogAccessRule CreateRule(MemberInfo memberInfo)
        {
            var logMessage = this.LogMessage;
            if (this.LogMessage == null)
            {
                logMessage = $"The property {memberInfo.Name} of {NamingUtilities.ToCSharpRepresentation(memberInfo.DeclaringType)} with id {{id}} was accessed";
            }

            return new LogAccessRule
                       {
                           Type = this.Type,
                           ConnectionActions = this.ConnectionActions,
                           Severity = this.Severity,
                           LogMessage = logMessage
            };
        }
    }
}
