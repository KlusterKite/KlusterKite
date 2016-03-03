// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerCollectorController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Manages request corresponding registered swagger services
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.Monitor.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    /// <summary>
    /// Manages request corresponding registered swagger services
    /// </summary>
    public class SwaggerCollectorController : ApiController
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// The actor system negotiation timeout
        /// </summary>
        private readonly TimeSpan systemTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerCollectorController"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public SwaggerCollectorController(ActorSystem system)
        {
            this.system = system;
            this.systemTimeout = system.Settings.Config.GetTimeSpan("ClusterKit.AskTimeout", TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gets current cluster member list
        /// </summary>
        /// <returns>The member list</returns>
        [Route("/swagger/monitor/getList")]
        [HttpGet, HttpPost]
        public async Task<IReadOnlyCollection<string>> GetServices()
        {
            try
            {
                return
                    await
                    this.system.ActorSelection("/Web/Swagger/Monitor")
                        .Ask<IReadOnlyCollection<string>>(
                            new SwaggerCollectorActor.SwaggerListRequest(),
                            this.systemTimeout);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}