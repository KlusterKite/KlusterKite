// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HostConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Nginx description of host configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.NginxConfigurator
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Nginx description of host configuration
    /// </summary>
    public class HostConfiguration : IEnumerable<ServiceConfiguration>
    {
        /// <summary>
        /// List of configured services
        /// </summary>
        private readonly Dictionary<string, ServiceConfiguration> services = new Dictionary<string, ServiceConfiguration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HostConfiguration"/> class.
        /// </summary>
        /// <param name="hostName">The virtual hostname to group services</param>
        public HostConfiguration(string hostName)
        {
            this.HostName = hostName;
        }

        /// <summary>
        /// Gets or sets the additional configuration
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Gets the configured services count
        /// </summary>
        public int Count => this.services.Count;

        /// <summary>
        /// Gets the virtual hostname of group services
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Gets the service description
        /// </summary>
        /// <param name="serviceName">The service name</param>
        /// <returns>The service description</returns>
        public ServiceConfiguration this[string serviceName]
        {
            get
            {
                if (!this.services.ContainsKey(serviceName))
                {
                    this.services[serviceName] = new ServiceConfiguration(serviceName);
                }

                return this.services[serviceName];
            }
        }

        /// <summary>
        /// Removes from cache services without active nodes
        /// </summary>
        public void Flush()
        {
            this.services.Values
                .Where(service => service.ActiveNodes.Count == 0 && string.IsNullOrWhiteSpace(service.Config))
                .Select(service => service.ServiceName).ToList()
                .ForEach(serviceName => this.services.Remove(serviceName));
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ServiceConfiguration> GetEnumerator()
        {
            return this.services.Values.ToList().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.services.Values.ToList().GetEnumerator();
        }
    }
}