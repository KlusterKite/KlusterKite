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
        ///
        /// </summary>
        private readonly Dictionary<string, HostConfiguration> hosts = new Dictionary<string, HostConfiguration>();

        public int Count => this.hosts.Count;

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

        public void Flush()
        {
            this.hosts.Values
                .Where(host => host.Count == 0 && string.IsNullOrWhiteSpace(host.Config))
                .Select(host => host.HostName).ToList()
                .ForEach(hostName => this.hosts.Remove(hostName));
        }

        public IEnumerator<HostConfiguration> GetEnumerator() => this.hosts.Values.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.hosts.Values.ToList().GetEnumerator();
    }
}