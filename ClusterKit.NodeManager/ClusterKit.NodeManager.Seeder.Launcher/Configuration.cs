// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The launcher configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Seeder.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;

    using Akka.Configuration;

    /// <summary>
    /// The launcher configuration
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the current execution framework
        /// </summary>
        public FrameworkName ExecutionFramework { get; set; }

        /// <summary>
        /// Gets or sets the configuration file name
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the parsed config
        /// </summary>
        public Config Config { get; set; }

        /// <summary>
        /// Gets or sets the nuget server address
        /// </summary>
        public string Nuget { get; set; }

        /// <summary>
        /// Gets or sets the nuget check period
        /// </summary>
        public TimeSpan NugetCheckPeriod { get; set; }

        /// <summary>
        /// Gets or sets the seeder configuration
        /// </summary>
        public List<SeederConfiguration> Configurations { get; set; }
    }
}
