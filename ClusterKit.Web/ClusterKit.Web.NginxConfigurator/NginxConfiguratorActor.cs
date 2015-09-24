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
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Policy;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;

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
        /// Initializes a new instance of the <see cref="NginxConfiguratorActor"/> class.
        /// </summary>
        public NginxConfiguratorActor()
        {
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
        }
    }
}