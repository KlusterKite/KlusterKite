// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDescriptorActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Standard actor, that discribed current node web services to other nodes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Descriptor
{
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Event;

    using JetBrains.Annotations;

    using KlusterKite.Web.Client.Messages;

    /// <summary>
    /// Standard actor, that describes current node web services to other nodes
    /// </summary>
    [UsedImplicitly]
    public class WebDescriptorActor : ReceiveActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDescriptorActor"/> class.
        /// </summary>
        public WebDescriptorActor()
        {
            var description = new WebDescriptionResponse();

            var config = Context?.System?.Settings?.Config;
            var servicesConfig = config?.GetConfig("KlusterKite.Web.Services");

            var services = new List<ServiceDescription>();

            if (servicesConfig != null)
            {
                foreach (var pair in servicesConfig.AsEnumerable())
                {
                    if (!pair.Value.IsObject())
                    {
                        Context.GetLogger().Error($"KlusterKite.Web.Services configuration is broken, {pair.Key} is not a valid configuration");
                        continue;
                    }

                    var serviceConfig = servicesConfig.GetConfig(pair.Key);
                    if (serviceConfig == null || serviceConfig.IsEmpty)
                    {
                        continue;
                    }

                    var serviceDescription = new ServiceDescription
                    {
                        ListeningPort = serviceConfig.GetInt("Port", 8080),
                        LocalHostName = serviceConfig.GetString("LocalHostName"),
                        PublicHostName = serviceConfig.GetString(
                                                             "PublicHostName",
                                                             "default"),
                        Route = serviceConfig.GetString("Route")
                    };

                    if (serviceDescription.Route == null)
                    {
                        Context.GetLogger().Error($"KlusterKite.Web.Services configuration is broken, {pair.Key} is not a valid configuration");
                        continue;
                    }

                    Context.GetLogger().Info($"Web publishing {pair.Key}: {serviceDescription.LocalHostName ?? "*"}:{serviceDescription.ListeningPort}/{serviceDescription.Route} for {serviceDescription.PublicHostName}");

                    services.Add(serviceDescription);
                }
            }

            description.Services = services.AsReadOnly();
            this.Receive<WebDescriptionRequest>(m => this.Sender.Tell(description));
        }
    }
}