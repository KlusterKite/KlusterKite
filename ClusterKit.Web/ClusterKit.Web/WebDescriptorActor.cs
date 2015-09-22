namespace ClusterKit.Web
{
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Standard actor, that discribed current node web services to other nodes
    /// </summary>
    [UsedImplicitly]
    public class WebDescriptorActor : ReceiveActor
    {
        /// <summary>
        /// Services, defined in config
        /// </summary>
        private readonly Dictionary<string, string> definedServices = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDescriptorActor"/> class.
        /// </summary>
        public WebDescriptorActor()
        {
            var config = Context.System.Settings.Config;
            var services = config.GetConfig("ClusterKit.Web.Services");
            foreach (var pair in config.AsEnumerable())
            {
                this.definedServices[pair.Key] = services.GetString(pair.Key, "default");
            }

            this.Receive<WebDescriptionRequest>(m => this.OnRequest());
        }

        /// <summary>
        /// Processes the request
        /// </summary>
        private void OnRequest()
        {
            var clone = this.definedServices.ToDictionary(p => p.Key, p => p.Value);
            this.Sender.Tell(new WebDescriptionResponse { ServiceNames = clone });
        }

        /// <summary>
        /// Request message, to get description of this node
        /// </summary>
        [UsedImplicitly]
        public class WebDescriptionRequest
        {
        }

        /// <summary>
        /// The message, that is sent as response to <seealso cref="WebDescriptionRequest"/>
        /// </summary>
        [UsedImplicitly]
        public class WebDescriptionResponse
        {
            /// <summary>
            /// Gets or sets the the list of services.
            /// </summary>
            /// <remarks>
            /// It doesn't supposed (but is not prohibited) that this should be public service hostname.
            /// It's just used to distinguish services with identical url paths to be correctly published on frontend web servers.
            /// </remarks>
            public Dictionary<string, string> ServiceNames { get; set; }
        }
    }
}