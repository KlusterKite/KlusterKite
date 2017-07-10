// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestController.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The testing web api controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Tests.Controllers
{
    using System;

    using KlusterKite.Security.Attributes;
    using KlusterKite.Web.Authorization;
    using KlusterKite.Web.Authorization.Attributes;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The testing web api controller
    /// </summary>
    [Route("authorization/test")]
    [RequireSession]
    public class TestController : Controller
    {
        /// <summary>
        /// Tests user authentication
        /// </summary>
        /// <returns>The user name</returns>
        [HttpGet]
        [Route("session")]
        public string GetUserSession()
        {
            var session = this.GetSession();
            return session.User?.UserId ?? session.ClientId;
        }

        /// <summary>
        /// Tests user authentication
        /// </summary>
        /// <returns>The user name</returns>
        [HttpGet]
        [RequireUser]
        [Route("user")]
        public string GetUser()
        {
            var session = this.GetSession();
            if (session.User == null)
            {
                throw new ArgumentNullException(nameof(AccessTicket.User));
            }

            return session.User.UserId;
        }

        /// <summary>
        /// User action authorized for default user on default client
        /// </summary>
        [HttpGet]
        [Route("AuthorizedUserAction")]
        [RequireUserPrivilege("User1")]
        [RequireClientPrivilege("Client1")]
        public void AuthorizedUserAction()
        {
        }

        /// <summary>
        /// User action unauthorized for default user, but authorized for default client
        /// </summary>
        [HttpGet]
        [Route("UnauthorizedUserAction")]
        [RequireUserPrivilege("User2")]
        [RequireClientPrivilege("Client1")]
        public void UnauthorizedUserAction()
        {
        }

        /// <summary>
        /// User action authorized for default user, but unauthorized for default client
        /// </summary>
        [HttpGet]
        [Route("UnauthorizedClientUserAction")]
        [RequireUserPrivilege("User1")]
        [RequireClientPrivilege("Client1-2")]
        public void UnauthorizedClientUserAction()
        {
        }

        /// <summary>
        /// User action authorized for either client or user
        /// </summary>
        [HttpGet]
        [Route("AuthorizedEitherClientUserAction")]
        [RequireUserPrivilege("User1", IgnoreOnClientOwnBehalf = true)]
        [RequireClientPrivilege("Client1-2", IgnoreOnUserPresent = true)]
        public void AuthorizedEitherClientUserAction()
        {
        }

        /// <summary>
        /// User action authorized with action name
        /// </summary>
        [HttpGet]
        [Route("authorizedUserExactAction")]
        [RequireUserPrivilege("User", CombinePrivilegeWithActionName = true)]
        [RequireClientPrivilege("Client", CombinePrivilegeWithActionName = true)]
        public void AuthorizedUserExactAction()
        {
        }

        /// <summary>
        /// User action authorized with action name
        /// </summary>
        [HttpGet]
        [Route("unauthorizedUserExactAction")]
        [RequireUserPrivilege("User1", CombinePrivilegeWithActionName = true)]
        [RequireClientPrivilege("Client1", CombinePrivilegeWithActionName = true)]
        public void UnauthorizedUserExactAction()
        {
        }
    }
}