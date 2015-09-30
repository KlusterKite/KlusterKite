namespace ClusterKit.Web.NginxConfigurator
{
    using System.Collections.Generic;

    public class ServiceConfiguration
    {
        public ServiceConfiguration(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        public List<string> ActiveNodes { get; private set; } = new List<string>();
        public string Config { get; set; }
        public string ServiceName { get; private set; }
    }
}