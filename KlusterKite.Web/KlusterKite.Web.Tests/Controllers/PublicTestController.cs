// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicTestController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The testing web api controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The testing web api controller
    /// </summary>
    [Route("authorization/public")]
    public class PublicTestController : Controller
    {
        /// <summary>
        /// Tests user authentication
        /// </summary>
        /// <returns>The user name</returns>
        [HttpGet]
        [Route("test")]
        public string GetUserSession()
        {
            return "Hello world";
        }
    }
}