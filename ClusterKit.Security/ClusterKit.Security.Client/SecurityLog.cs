// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityLog.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Logging utilities
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.Core.Log;

    using Serilog.Events;

    using LogEvent = Serilog.Events.LogEvent;

    /// <summary>
    /// Logging utilities
    /// </summary>
    public static class SecurityLog
    {
        /// <summary>
        /// The log record type
        /// </summary>
        public enum EnType
        {
            /// <summary>
            /// Data read operation was successful
            /// </summary>
            DataReadGranted,

            /// <summary>
            /// Data create operation was successful
            /// </summary>
            DataCreateGranted,

            /// <summary>
            /// Data update / change operation was successful
            /// </summary>
            DataUpdateGranted,

            /// <summary>
            /// Data removal operation was successful
            /// </summary>
            DataDeleteGranted,

            /// <summary>
            /// Some uncategorized operation was successfully made
            /// </summary>
            OperationGranted,

            /// <summary>
            /// Authentication grant was successful
            /// </summary>
            AuthenticationGranted,

            /// <summary>
            /// Unsuccessful authentication attempt
            /// </summary>
            AuthenticationDenied,

            /// <summary>
            /// Attempt to make unauthorized operation
            /// </summary>
            OperationDenied,
        }

        /// <summary>
        /// Creates a record to the security log
        /// </summary>
        /// <param name="recordType">The record type</param>
        /// <param name="severity">The data severity</param>
        /// <param name="requestDescription">The request description</param>
        /// <param name="format">The message format</param>
        /// <param name="parameters">Additional message parameters</param>
        public static void CreateRecord(
            EnType recordType,
            EnSeverity severity,
            RequestDescription requestDescription,
            string format,
            params object[] parameters)
        {
            MessageTemplate template;
            IEnumerable<LogEventProperty> properties;
            if (!Serilog.Log.BindMessageTemplate(format, parameters, out template, out properties))
            {
                throw new InvalidOperationException("Incorrect message format");
            }

            var record = new LogEvent(DateTimeOffset.Now, GetLogLevel(recordType, severity), null, template, properties);
            record.AddOrUpdateProperty(
                new LogEventProperty(Constants.LogRecordTypeKey, new ScalarValue(EnLogRecordType.Security)));
            record.AddPropertyIfAbsent(new LogEventProperty("SecurityRecordType", new ScalarValue(recordType)));
            record.AddPropertyIfAbsent(new LogEventProperty("SecuritySeverity", new ScalarValue(severity)));

            if (requestDescription != null)
            {
                record.AddPropertyIfAbsent(new LogEventProperty("SecurityRequest", CreateLogValue(requestDescription)));
            }

            Serilog.Log.Logger.Write(record);
        }

        /// <summary>
        /// Creates log description from the request description
        /// </summary>
        /// <param name="requestDescription">The request description</param>
        /// <returns>The log description</returns>
        private static LogEventPropertyValue CreateLogValue(RequestDescription requestDescription)
        {
            var dictionary = new Dictionary<ScalarValue, LogEventPropertyValue>();

            if (!string.IsNullOrWhiteSpace(requestDescription.RemoteAddress))
            {
                dictionary[new ScalarValue("RemoteAddress")] = new ScalarValue(requestDescription.RemoteAddress);
            }

            if (requestDescription.Headers != null)
            {
                dictionary[new ScalarValue("Headers")] =
                    new DictionaryValue(
                        requestDescription.Headers.Select(
                            p =>
                                new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                                    new ScalarValue(p.Key),
                                    new ScalarValue(p.Value))));
            }

            if (requestDescription.Authentication != null)
            {
                var auth = requestDescription.Authentication;
                var authenticationDictionary = new Dictionary<ScalarValue, LogEventPropertyValue>();
                if (auth.User != null)
                {
                    authenticationDictionary[new ScalarValue("UserId")] = new ScalarValue(auth.User.UserId);
                }

                authenticationDictionary[new ScalarValue("ClientId")] = new ScalarValue(auth.ClientId);
                authenticationDictionary[new ScalarValue("ClientType")] = new ScalarValue(auth.ClientType);
                authenticationDictionary[new ScalarValue("Created")] = new ScalarValue(auth.Created);
                authenticationDictionary[new ScalarValue("Expiring")] = new ScalarValue(auth.Expiring);

                dictionary[new ScalarValue("Authentication")] = new DictionaryValue(authenticationDictionary);
            }

            return new DictionaryValue(dictionary);
        }

        /// <summary>
        /// Gets the log record level by it's type and severity
        /// </summary>
        /// <param name="recordType">The record type</param>
        /// <param name="severity">The data severity</param>
        /// <returns>The log record level</returns>
        private static LogEventLevel GetLogLevel(EnType recordType, EnSeverity severity)
        {
            switch (severity)
            {
                case EnSeverity.Trivial:
                    switch (recordType)
                    {
                        case EnType.DataReadGranted:
                        case EnType.OperationGranted:
                            return LogEventLevel.Debug;
                        case EnType.DataCreateGranted:
                        case EnType.DataUpdateGranted:
                        case EnType.DataDeleteGranted:
                        case EnType.AuthenticationGranted:
                            return LogEventLevel.Information;
                        case EnType.AuthenticationDenied:
                            return LogEventLevel.Warning;
                        case EnType.OperationDenied:
                            return LogEventLevel.Error;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(recordType), recordType, null);
                    }

                case EnSeverity.Crucial:
                    switch (recordType)
                    {
                        case EnType.DataReadGranted:
                        case EnType.OperationGranted:
                        case EnType.DataCreateGranted:
                        case EnType.DataUpdateGranted:
                        case EnType.DataDeleteGranted:
                        case EnType.AuthenticationGranted:
                            return LogEventLevel.Information;
                        case EnType.AuthenticationDenied:
                            return LogEventLevel.Warning;
                        case EnType.OperationDenied:
                            return LogEventLevel.Error;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(recordType), recordType, null);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }
    }
}