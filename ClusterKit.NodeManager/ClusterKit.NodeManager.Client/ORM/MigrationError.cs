// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationError.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The error description during the resource check and/or migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The error description during the resource check and/or migration
    /// </summary>
    [ApiDescription("The error description during the resource check and/or migration", Name = "MigrationError")]
    [Table("MigrationErrors")]
#if APPDOMAIN
    [Serializable]
#endif
    public class MigrationError : MigrationLogRecord
    {
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        [DeclareField("the error occurrence time")]
        [UsedImplicitly]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [DeclareField("the resource name")]
        [UsedImplicitly]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error stack trace
        /// </summary>
        [DeclareField("the error stack trace")]
        [UsedImplicitly]
        public string ErrorStackTrace { get; set; }

        /// <summary>
        /// Sets the description from exception
        /// </summary>
        public Exception Exception
        {
            set
            {
                this.ErrorStackTrace = string.Empty;
                this.AddExceptionToStackTrace(value);
            }
        }

        /// <summary>
        /// Adds exception description to <see cref="ErrorStackTrace"/>
        /// </summary>
        /// <param name="value">The exception</param>
        private void AddExceptionToStackTrace(Exception value)
        {
#if APPDOMAIN
            this.ErrorStackTrace += $"{value.GetType().Name}: {value.Message}\n{value.StackTrace}\n{(value as System.IO.FileNotFoundException)?.FusionLog}";
#endif
#if CORECLR
            this.ErrorStackTrace += $"{value.GetType().Name}: {value.Message}\n{value.StackTrace}";
#endif

            if (value.InnerException != null)
            {
                this.ErrorStackTrace += "\n---------------------\n";
                this.AddExceptionToStackTrace(value.InnerException);
            }
        }
    }
}
