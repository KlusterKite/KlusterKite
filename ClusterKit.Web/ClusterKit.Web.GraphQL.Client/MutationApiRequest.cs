// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MutationApiRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request for mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    /// <summary>
    /// The request for mutation
    /// </summary>
    public class MutationApiRequest : ApiRequest
    {
        /// <summary>
        /// Gets a value indicating whether this request is a mutation request
        /// </summary>
        public bool IsMutation => true;
    }
}