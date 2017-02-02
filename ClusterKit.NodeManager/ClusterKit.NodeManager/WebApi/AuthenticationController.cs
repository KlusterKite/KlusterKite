// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Authenticate web api users
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System.Net;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Web.Authorization;

    /// <summary>
    /// Authenticate web api users
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/authentication")]
    public class AuthenticationController : ApiController
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="system">
        /// The actor system.
        /// </param>
        public AuthenticationController(ActorSystem system)
        {
            this.system = system;
        }

        /// <summary>
        /// Gets the currently authenticated user name
        /// </summary>
        /// <returns>The user name</returns>
        [Route("user")]
        [HttpGet]
        public UserDescription GetUser()
        {
            var session = this.GetSession();
            if (session == null)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var description = session.User as UserDescription;

            if (description == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return description;
        }
    }
}
