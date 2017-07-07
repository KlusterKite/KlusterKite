// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeederConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The configuration for the seeder instance
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Seeder.Launcher
{
    using System.Collections.Generic;

    using Akka.Configuration;

    /// <summary>
    /// The configuration for the seeder instance
    /// </summary>
    public class SeederConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeederConfiguration"/> class.
        /// </summary>
        /// <param name="name">The seeder configuration name</param>
        /// <param name="config">
        /// The seeder configuration config.
        /// </param>
        public SeederConfiguration(string name, Config config)
        {
            this.RequiredPackages = config.GetStringList("RequiredPackages");
            this.Seeders = config.GetStringList("Seeders");
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of configuration
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the list of required packages
        /// </summary>
        public IList<string> RequiredPackages { get; }

        /// <summary>
        /// Gets the list of seeders
        /// </summary>
        public IList<string> Seeders { get; }
    }
}
