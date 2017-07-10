// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Collection of objects request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD.ActionMessages
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Akka.Routing;

    using JetBrains.Annotations;

    using KlusterKite.API.Client;
    using KlusterKite.Security.Attributes;

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
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets some extra data, that will be returned with the response
        /// </summary>
        [UsedImplicitly]
        public byte[] ExtraData { get; set; }

        /// <summary>
        /// Gets or sets the original <see cref="ApiRequest"/>. Optional.
        /// </summary>
        public ApiRequest ApiRequest { get; set; }

        /// <summary>
        /// Gets or sets the original requester description
        /// </summary>
        /// <remarks>
        /// This can be used for further authorization checks
        /// </remarks>
        public RequestContext RequestContext { get; set; }

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
        /// <summary>
        /// Gets or sets the filtering condition
        /// </summary>
        public Expression<Func<TObject, bool>> Filter { get; set; }

        /// <summary>
        /// Gets or sets the sorting function
        /// </summary>
        public List<SortingCondition> Sort { get; set; }
    }
}