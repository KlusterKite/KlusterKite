// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Collection of objects request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Rest.ActionMessages
{
    using System;

    using Akka.Routing;

    /// <summary>
    /// Collection of objects request
    /// </summary>
    public class CollectionRequest : IConsistentHashable
    {
        /// <summary>
        /// The consistent hash key of the marked class.
        /// </summary>
        /// <remarks>
        /// Collection request can't be assigned to specific id
        /// </remarks>
        public object ConsistentHashKey => Guid.NewGuid();

        /// <summary>
        /// Gets or sets the maximum number of objects to return.
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the number of objects to skip in select
        /// </summary>
        public int Skip { get; set; } = 0;
    }
}