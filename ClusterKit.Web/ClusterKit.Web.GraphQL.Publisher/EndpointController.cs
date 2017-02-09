// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   GraphQL endpoint controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Web.Http;
    
    /// <summary>
    /// GraphQL endpoint controller
    /// </summary>
    [RoutePrefix("api/1.x/graphQL")]
    public class EndpointController : ApiController
    {
        /// <summary>
        /// The test request
        /// </summary>
        /// <returns>The test object</returns>
        [Route("test")]
        [HttpGet]
        public object Test()
        {
            return null;
        }
    }
}
