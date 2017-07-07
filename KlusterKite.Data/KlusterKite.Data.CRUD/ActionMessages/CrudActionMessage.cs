// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrudActionMessage.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Request to process som action
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.ActionMessages
{
    using Akka.Routing;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Request to process some data action
    /// </summary>
    /// <typeparam name="TData">The type of data object</typeparam>
    /// <typeparam name="TId">The type of data object identification</typeparam>
    public class CrudActionMessage<TData, TId> : IConsistentHashable, ICrudActionMessage, IMessageWithExtraData
    {
        /// <summary>
        /// Gets or sets the type of request
        /// </summary>
        public EnActionType ActionType { get; set; }

        /// <summary>
        /// The consistent hash key of the marked class.
        /// </summary>
        public object ConsistentHashKey => this.Id.GetHashCode();

        /// <summary>
        /// Gets or sets the identification of object.
        /// </summary>
        [UsedImplicitly]
        public TId Id { get; set; }

        /// <summary>
        /// Gets or sets the object itself.
        /// </summary>
        [UsedImplicitly]
        public TData Data { get; set; }

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
        /// Gets or sets some extra data, that will be returned with the response
        /// </summary>
        [UsedImplicitly]
        public byte[] ExtraData { get; set; }

        /// <inheritdoc />
        EnActionType ICrudActionMessage.ActionType => this.ActionType;

        /// <inheritdoc />
        object ICrudActionMessage.Id => this.Id;

        /// <inheritdoc />
        object ICrudActionMessage.Data => this.Data;

        /// <inheritdoc />
        byte[] ICrudActionMessage.ExtraData => this.ExtraData;
    }
}