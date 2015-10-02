// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PingMeasurement.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The result of ping measurment
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Messages
{
    using System;

    using Akka.Actor;

    /// <summary>
    /// The result of ping measurement
    /// </summary>
    public class PingMeasurement
    {
        /// <summary>
        /// Gets or sets the address of tested node
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Gets or sets the result of measurement. Null in case of timeout or no response
        /// </summary>
        public TimeSpan? Result { get; set; }
    }
}