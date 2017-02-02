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
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization;
    using ClusterKit.Web.Authorization.Attributes;

    /// <summary>
    /// Authenticate web api users
    /// </summary>
    [RoutePrefix("api/1.x/clusterkit/nodemanager/authentication")]
    [RequireUser]
    public class AuthenticationController : ApiController
    {
        /// <summary>
        /// Gets the currently authenticated user name
        /// </summary>
        /// <returns>The user name</returns>
        [Route("user")]
        [HttpGet]
        public UserDescription GetUser()
        {
            var session = this.GetSession();
            var description = session.User as UserDescription;

            if (description == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return description;
        }

        /// <summary>
        /// Gets the currently authenticated user scope list (the list of granted privileges)
        /// </summary>
        /// <returns>The user name</returns>
        [Route("userScope")]
        [HttpGet]
        public IEnumerable<string> GetMyScope()
        {
            var session = this.GetSession();
            return session.UserScope;
        }

        /// <summary>
        /// Gets the list of the defined privileges
        /// </summary>
        /// <returns>the list of the defined privileges</returns>
        [Route("definedPrivileges")]
        [HttpGet]
        [RequireUserPrivilege(Client.Privileges.GetPrivilegesList)]
        public IEnumerable<PrivilegeDescription> GetRegisteredPrivileges()
        {
            return Utils.DefinedPrivileges;
        }

        /// <summary>
        /// Renews the expiry of the token
        /// </summary>
        /// <returns>The renewed token</returns>
        [Route("renewToken")]
        [HttpGet]
        public Task<TokenResponse> RefreshToken()
        {
            return Task.FromResult<TokenResponse>(null);
        }
    }
}
