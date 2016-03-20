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
        private readonly Dictionary<string, ServiceConfiguration> services = new Dictionary<string, ServiceConfiguration>();

        public HostConfiguration(string hostName)
        {
            this.HostName = hostName;
        }

        public string Config { get; set; }

        public int Count => this.services.Count;

        public string HostName { get; private set; }

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

        public void Flush()
        {
            this.services.Values
                .Where(service => service.ActiveNodes.Count == 0 && string.IsNullOrWhiteSpace(service.Config))
                .Select(service => service.ServiceName).ToList()
                .ForEach(serviceName => this.services.Remove(serviceName));
        }

        public IEnumerator<ServiceConfiguration> GetEnumerator()
        {
            return this.services.Values.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.services.Values.ToList().GetEnumerator();
        }
    }
}