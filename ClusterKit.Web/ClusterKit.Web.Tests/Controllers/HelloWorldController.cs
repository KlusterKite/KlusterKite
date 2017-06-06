// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelloWorldController.cs" company="ClusterKit">
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
    [Route("testController")]
    public class HelloWorldController : Controller
    {
        /// <summary>
        /// Tests simple string method
        /// </summary>
        /// <returns>The test string</returns>
        [HttpGet]
        [Route("method")]
        public IActionResult HelloWorld()
        {
            return this.Ok("Hello world");
        }
    }
}