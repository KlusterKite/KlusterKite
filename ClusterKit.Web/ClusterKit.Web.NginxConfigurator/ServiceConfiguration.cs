namespace ClusterKit.Web.NginxConfigurator
{
    using System.Collections.Generic;

    /// <summary>
    /// Published web service configuration description for Nginx
    /// </summary>
    public class ServiceConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceConfiguration"/> class.
        /// </summary>
        public ServiceConfiguration(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        /// <summary>
        /// Gets the list of private nodes
        /// </summary>
        public List<NodeServiceConfiguration> ActiveNodes { get; private set; } = new List<NodeServiceConfiguration>();

        /// <summary>
        /// Gets or sets current location nginx configuration
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Gets current service name
        /// </summary>
        public string ServiceName { get; private set; }
    }
}