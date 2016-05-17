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

    using JetBrains.Annotations;

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
        public virtual object ConsistentHashKey => Guid.NewGuid();

        /// <summary>
        /// Gets or sets the maximum number of objects to return.
        /// </summary>
        [UsedImplicitly]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the number of objects to skip in select
        /// </summary>
        [UsedImplicitly]
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets some extra data, that will be returned with the response
        /// </summary>
        [UsedImplicitly]
        public object ExtraData { get; set; }
    }

    /// <summary>
    /// Collection of objects request
    /// </summary>
    [UsedImplicitly]
    // ReSharper disable once UnusedTypeParameter
    public class CollectionRequest<TObject> : CollectionRequest
    {
    }
}