// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDescriptorActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Standard actor, that discribed current node web services to other nodes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Akka.Actor;

    using ClusterKit.Web.Client;
    using ClusterKit.Web.Client.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Standard actor, that described current node web services to other nodes
    /// </summary>
    [UsedImplicitly]
    public class WebDescriptorActor : ReceiveActor
    {
        /// <summary>
        /// Services, defined in config
        /// </summary>
        private readonly Dictionary<string, string> definedServices = new Dictionary<string, string>();

        /// <summary>
        /// The port, where web service is listening connections
        /// </summary>
        private int listeningPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDescriptorActor"/> class.
        /// </summary>
        public WebDescriptorActor()
        {
            var config = Context.System.Settings.Config;
            var services = config.GetConfig("ClusterKit.Web.Services");
            foreach (var pair in services.AsEnumerable())
            {
                this.definedServices[pair.Key] = services.GetString(pair.Key, "default");
            }

            var uri = Installer.GetOwinBindUrl(Context.System.Settings.Config);

            this.listeningPort = 80;
            var result = Regex.Match(uri, "^(http:\\/\\/)?(?<host>([a-z\\.\\-0-9]+|\\*)):(?<port>[0-9]+)(\\/[a-z0-9\\.\\-])?$");
            if (result.Success && result.Groups["port"] != null)
            {
                this.listeningPort = int.Parse(result.Groups["port"].Value);
            }

            this.Receive<WebDescriptionRequest>(m => this.OnRequest());
        }

        /// <summary>
        /// Processes the request
        /// </summary>
        private void OnRequest()
        {
            var clone = this.definedServices.ToDictionary(p => p.Key, p => p.Value);
            this.Sender.Tell(new WebDescriptionResponse { ServiceNames = clone, ListeningPort = this.listeningPort });
        }
    }
}