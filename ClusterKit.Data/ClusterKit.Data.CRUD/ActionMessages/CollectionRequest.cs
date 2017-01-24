// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Collection of objects request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.ActionMessages
{
    using System;

    using Akka.Routing;

    using JetBrains.Annotations;

    /// <summary>
    /// Collection of objects request
    /// </summary>
    public class CollectionRequest : IConsistentHashable, IMessageWithExtraData
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
        public byte[] ExtraData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response is acceptable as parcel.
        /// </summary>
        [UsedImplicitly]
        public bool AcceptAsParcel { get; set; } = true;
    }

    /// <summary>
    /// Collection of objects request
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of the retrieved object
    /// </typeparam>
    [UsedImplicitly]
    // ReSharper disable once UnusedTypeParameter
    // ReSharper disable once StyleCop.SA1402
    public class CollectionRequest<TObject> : CollectionRequest
    {
    }
}