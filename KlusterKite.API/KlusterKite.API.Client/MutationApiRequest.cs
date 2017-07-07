// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MutationApiRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The request for mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// The request for mutation
    /// </summary>
    public class MutationApiRequest : ApiRequest
    {
        /// <summary>
        /// Gets a value indicating whether this request is a mutation request
        /// </summary>
        public bool IsMutation => true;

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}