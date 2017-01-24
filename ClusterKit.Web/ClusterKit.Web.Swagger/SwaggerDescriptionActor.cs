// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerDescriptionActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Sends information of current published swagger
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger
{
    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Web.Swagger.Messages;

    /// <summary>
    /// Sends information of current published swagger
    /// </summary>
    public class SwaggerDescriptionActor : ReceiveActor
    {
        /// <summary>
        /// Current swagger description
        /// </summary>
        private readonly SwaggerPublishDescription description;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerDescriptionActor"/> class.
        /// </summary>
        public SwaggerDescriptionActor()
        {
            var publishDocUrl = Context.System.Settings.Config.GetString("ClusterKit.Web.Swagger.Publish.publishDocPath", "swagger");
            var publishUiUrl = Context.System.Settings.Config.GetString("ClusterKit.Web.Swagger.Publish.publishUiPath", "swagger/ui");

            this.description = new SwaggerPublishDescription
            {
                Url = publishUiUrl,
                DocUrl = publishDocUrl
            };

            Cluster.Get(Context.System)
                .Subscribe(
                    this.Self,
                    ClusterEvent.InitialStateAsEvents, 
                    typeof(ClusterEvent.MemberUp));

            this.Receive<ClusterEvent.MemberUp>(
                m => m.Member.Roles.Contains("Web.Swagger.Monitor"),
                m => this.OnNodeUp(m.Member.Address));

            this.Receive<SwaggerPublishDescriptionRequest>(m => this.OnNodeDescriptionRequest());
        }

        /// <summary>
        /// Processes the swagger description request
        /// </summary>
        private void OnNodeDescriptionRequest()
        {
            this.Sender.Tell(this.description);
        }

        /// <summary>
        /// Handles the event of swagger monitoring node up
        /// </summary>
        /// <param name="address">Address of the node</param>
        private void OnNodeUp(Address address)
        {
            Context.ActorSelection($"{address}/user/Web/Swagger/Monitor").Tell(this.description);
        }
    }
}