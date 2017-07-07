// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Nginx sites configuration description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.NginxConfigurator
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Nginx sites configuration description
    /// </summary>
    public class WebConfiguration : IEnumerable<HostConfiguration>
    {
        /// <summary>
        /// The list of cached hosts
        /// </summary>
        private readonly Dictionary<string, HostConfiguration> hosts = new Dictionary<string, HostConfiguration>();

        /// <summary>
        /// Gets the configured hosts count
        /// </summary>
        public int Count => this.hosts.Count;

        /// <summary>
        /// Gets the host configuration by it's name
        /// </summary>
        /// <param name="hostName">The host's name</param>
        /// <returns>The host configuration</returns>
        public HostConfiguration this[string hostName]
        {
            get
            {
                if (!this.hosts.ContainsKey(hostName))
                {
                    this.hosts[hostName] = new HostConfiguration(hostName);
                }

                return this.hosts[hostName];
            }
        }

        /// <summary>
        /// Removes empty host configurations from cache
        /// </summary>
        public void Flush()
        {
            this.hosts.Values
                .Where(host => host.Count == 0 && string.IsNullOrWhiteSpace(host.Config))
                .Select(host => host.HostName).ToList()
                .ForEach(hostName => this.hosts.Remove(hostName));
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<HostConfiguration> GetEnumerator() => this.hosts.Values.ToList().GetEnumerator();

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.hosts.Values.ToList().GetEnumerator();
    }
}