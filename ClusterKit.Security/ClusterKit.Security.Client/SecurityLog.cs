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
    using ClusterKit.Security.Attributes;

    using Newtonsoft.Json;

    using Serilog.Events;

    /// <summary>
    /// Logging utilities
    /// </summary>
    public static class SecurityLog
    {
        /// <summary>
        /// Creates a record to the security log
        /// </summary>
        /// <param name="recordType">The record type</param>
        /// <param name="severity">The data severity</param>
        /// <param name="requestContext">The request description</param>
        /// <param name="format">The message format</param>
        /// <param name="parameters">Additional message parameters</param>
        public static void CreateRecord(
            EnSecurityLogType recordType,
            EnSeverity severity,
            RequestContext requestContext,
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

            if (requestContext != null)
            {
                record.AddPropertyIfAbsent(new LogEventProperty("SecurityRequest", CreateLogValue(requestContext)));
            }

            Serilog.Log.Logger.Write(record);
        }

        /// <summary>
        /// Creates log description from the request description
        /// </summary>
        /// <param name="requestContext">The request description</param>
        /// <returns>The log description</returns>
        private static LogEventPropertyValue CreateLogValue(RequestContext requestContext)
        {
            var dictionary = new Dictionary<ScalarValue, LogEventPropertyValue>();

            if (!string.IsNullOrWhiteSpace(requestContext.RemoteAddress))
            {
                dictionary[new ScalarValue("RemoteAddress")] = new ScalarValue(requestContext.RemoteAddress);
            }

            if (requestContext.Headers != null)
            {
                dictionary[new ScalarValue("Headers")] =
                    new DictionaryValue(
                        requestContext.Headers.Select(
                            p =>
                                new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                                    new ScalarValue(p.Key),
                                    new ScalarValue(p.Value))));
            }

            if (!string.IsNullOrWhiteSpace(requestContext.RequestedLocalUrl))
            {
                dictionary[new ScalarValue("LocalUrl")] = new ScalarValue(requestContext.RequestedLocalUrl);
            }

            if (requestContext.Authentication != null)
            {
                var auth = requestContext.Authentication;
                var authenticationDictionary = new Dictionary<ScalarValue, LogEventPropertyValue>();
                if (auth.User != null)
                {
                    authenticationDictionary[new ScalarValue("UserId")] = new ScalarValue(auth.User.UserId);
                    authenticationDictionary[new ScalarValue("UserData")] = new ScalarValue(JsonConvert.SerializeObject(auth.User));
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
        private static LogEventLevel GetLogLevel(EnSecurityLogType recordType, EnSeverity severity)
        {
            switch (severity)
            {
                case EnSeverity.Trivial:
                    switch (recordType)
                    {
                        case EnSecurityLogType.DataReadGranted:
                        case EnSecurityLogType.OperationGranted:
                            return LogEventLevel.Debug;
                        case EnSecurityLogType.DataCreateGranted:
                        case EnSecurityLogType.DataUpdateGranted:
                        case EnSecurityLogType.DataDeleteGranted:
                        case EnSecurityLogType.AuthenticationGranted:
                            return LogEventLevel.Information;
                        case EnSecurityLogType.AuthenticationDenied:
                            return LogEventLevel.Warning;
                        case EnSecurityLogType.OperationDenied:
                            return LogEventLevel.Error;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(recordType), recordType, null);
                    }

                case EnSeverity.Crucial:
                    switch (recordType)
                    {
                        case EnSecurityLogType.DataReadGranted:
                        case EnSecurityLogType.OperationGranted:
                        case EnSecurityLogType.DataCreateGranted:
                        case EnSecurityLogType.DataUpdateGranted:
                        case EnSecurityLogType.DataDeleteGranted:
                        case EnSecurityLogType.AuthenticationGranted:
                            return LogEventLevel.Information;
                        case EnSecurityLogType.AuthenticationDenied:
                            return LogEventLevel.Warning;
                        case EnSecurityLogType.OperationDenied:
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