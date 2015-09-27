// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NginxConfiguratorActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Follows cluster changes for adding / removing new nodes with "web" role and configures local nginx for supported urls
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.NginxConfigurator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;

    using ClusterKit.Web.Client;
    using ClusterKit.Web.Client.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Follows cluster changes for adding / removing new nodes with "web" role and configures local nginx for supported services
    /// </summary>
    [UsedImplicitly]
    public class NginxConfiguratorActor : ReceiveActor
    {
        /// <summary>
        /// Current configuration file path
        /// </summary>
        private string configPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="NginxConfiguratorActor"/> class.
        /// </summary>
        public NginxConfiguratorActor()
        {
            this.configPath = Context.System.Settings.Config.GetString("ClusterKit.Web.Nginx.PathToConfig");
            Cluster.Get(Context.System)
                .Subscribe(
                    this.Self,
                    ClusterEvent.InitialStateAsEvents,
                    new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.RoleLeaderChanged) });

            this.Receive<ClusterEvent.MemberUp>(
                m => m.Member.Roles.Contains("Web"),
                m => this.OnWebNodeUp(m.Member.Address));

            this.Receive<ClusterEvent.MemberRemoved>(
                m => m.Member.Roles.Contains("Web"),
                m => this.OnWebNodeDown(m.Member.Address));

            this.Receive<WebDescriptionResponse>(r => this.OnNewNodeDescription(r));
        }

        /// <summary>
        /// Gets nodes configuration description
        /// </summary>
        public Dictionary<string, Dictionary<string, List<string>>> ConfigDictionary { get; } = new Dictionary<string, Dictionary<string, List<string>>>();

        /// <summary>
        /// Gets the list of known active web nodes addresses
        /// </summary>
        public List<Address> KnownActiveNodes { get; } = new List<Address>();

        /// <summary>
        /// Gets cahed data of published web urls in everey known node
        /// </summary>
        public Dictionary<Address, string> NodePublishUrls { get; } = new Dictionary<Address, string>();

        /// <summary>
        /// Compiles upstream name from hostname and servicename
        /// </summary>
        /// <param name="hostName">Host name of service</param>
        /// <param name="serviceName">Service location</param>
        /// <returns>The corresponding upstream name</returns>
        private string GetUpStreamName([NotNull] string hostName, [NotNull] string serviceName)
        {
            if (hostName == null)
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            return $"ClusterKitWeb_{hostName.Replace('.', '_')}_{serviceName.Replace('/', '_').Replace('.', '_')}";
        }

        /// <summary>
        /// Applies node description to configuration
        /// </summary>
        /// <param name="description">The node description</param>
        private void OnNewNodeDescription(WebDescriptionResponse description)
        {
            var nodeAddress = this.Sender.Path.Address;
            if (nodeAddress.Host == null)
            {
                // supposed this is local address
                nodeAddress = Cluster.Get(Context.System).SelfAddress;
            }

            if (!this.KnownActiveNodes.Contains(nodeAddress))
            {
                // node managed to go down before it was initialized
                return;
            }

            if (this.NodePublishUrls.ContainsKey(nodeAddress))
            {
                // duplicate configuration info
                return;
            }

            var nodeUrl = $"{nodeAddress.Host}:{description.ListeningPort}";
            this.NodePublishUrls[nodeAddress] = nodeUrl;

            foreach (var serviceDescription in description.ServiceNames)
            {
                var serviceHost = serviceDescription.Value;
                var servicePath = serviceDescription.Key;

                if (!this.ConfigDictionary.ContainsKey(serviceHost))
                {
                    this.ConfigDictionary[serviceHost] = new Dictionary<string, List<string>>();
                }

                if (!this.ConfigDictionary[serviceHost].ContainsKey(servicePath))
                {
                    this.ConfigDictionary[serviceHost][servicePath] = new List<string>();
                }

                if (!this.ConfigDictionary[serviceHost][servicePath].Contains(nodeUrl))
                {
                    this.ConfigDictionary[serviceHost][servicePath].Add(nodeUrl);
                }
            }

            this.WriteConfiguration();
        }

        /// <summary>
        /// Removes all references for node from configuration
        /// </summary>
        /// <param name="nodeAddress">The node address</param>
        private void OnWebNodeDown(Address nodeAddress)
        {
            this.KnownActiveNodes.Remove(nodeAddress);
            string nodeUrl;
            if (!this.NodePublishUrls.TryGetValue(nodeAddress, out nodeUrl))
            {
                // something sttrange. Local data is corrupted;
                return;
            }

            foreach (var hostPair in this.ConfigDictionary.ToList())
            {
                var hostName = hostPair.Key;
                foreach (var servicePair in hostPair.Value)
                {
                    var serviceName = servicePair.Key;
                    servicePair.Value.Remove(nodeUrl);

                    if (servicePair.Value.Count == 0)
                    {
                        this.ConfigDictionary[hostName].Remove(serviceName);
                    }
                }

                if (hostPair.Value.Count == 0)
                {
                    this.ConfigDictionary.Remove(hostName);
                }
            }

            this.NodePublishUrls.Remove(nodeAddress);

            this.WriteConfiguration();
        }

        /// <summary>
        /// Requests the node configuration for newly attached node
        /// </summary>
        /// <param name="nodeAddress">The node address</param>
        private void OnWebNodeUp(Address nodeAddress)
        {
            if (!this.KnownActiveNodes.Contains(nodeAddress))
            {
                this.KnownActiveNodes.Add(nodeAddress);
            }

            Context.System.GetWebDescriptor(nodeAddress)
                .Tell(new WebDescriptionRequest(), this.Self);
        }

        /// <summary>
        /// Writes current configuration to nginx config file and sends reload to nginx
        /// </summary>
        private void WriteConfiguration()
        {
            StringBuilder config = new StringBuilder();
            var akkaConfig = Context.System.Settings.Config.GetConfig("ClusterKit.Web.Nginx.ServicesHost");

            this.WriteUpStreamsToConfig(config);
            this.WriteUpStreamsToConfig(config);
            this.WriteServicesToConfig(akkaConfig, config);

            File.WriteAllText(this.configPath, config.ToString());
        }

        /// <summary>
        /// Writes every defined service to nginx config
        /// </summary>
        /// <param name="akkaConfig">Current node configuration</param>
        /// <param name="config">Configuration file to write</param>
        private void WriteServicesToConfig(Config akkaConfig, StringBuilder config)
        {
            foreach (var hostPair in this.ConfigDictionary)
            {
                var hostName = hostPair.Key;
                var hostConfig = akkaConfig.GetConfig(hostName) ?? ConfigurationFactory.Empty;
                config.Append("server {\n");
                config.Append($"\tlisten {hostConfig.GetString("listen", "80")}\n");
                var servername = hostConfig.GetString("server_name");
                if (servername != null)
                {
                    config.Append($"\tserver_name {servername}\n");
                }

                foreach (var servicePair in hostPair.Value)
                {
                    var serviceName = servicePair.Key;
                    config.Append($"\tlocation {serviceName} {{\n");
                    config.Append($"\t\tproxy_pass http://{this.GetUpStreamName(hostName, serviceName)}\n");
                    config.Append("\t}\n");
                }
                config.Append("}\n");
            }
        }

        /// <summary>
        /// Writes every defined upstream for every defined service to nginx config
        /// </summary>
        /// <param name="config">Configuration file to write</param>
        private void WriteUpStreamsToConfig(StringBuilder config)
        {
            foreach (var hostPair in this.ConfigDictionary)
            {
                var hostName = hostPair.Key;
                foreach (var servicePair in hostPair.Value)
                {
                    var serviceName = servicePair.Key;
                    config.Append(
                        $@"
upstream {this.GetUpStreamName(hostName, serviceName)} {{
{
                            string.Join("\n", servicePair.Value.Select(u => $"\tserver {u};"))}
}}
");
                }
            }
        }
    }
}