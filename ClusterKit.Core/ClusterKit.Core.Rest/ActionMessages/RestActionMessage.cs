// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestActionMessage.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Request to process som action
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Rest.ActionMessages
{
    using Akka.Routing;

    /// <summary>
    /// Request to process some action
    /// </summary>
    /// <typeparam name="TData">The type of data object</typeparam>
    /// <typeparam name="TId">The type of data object identification</typeparam>
    public class RestActionMessage<TData, TId> : IConsistentHashable
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
        public TId Id { get; set; }

        /// <summary>
        /// Gets or sets the object itself.
        /// </summary>
        public TData Request { get; set; }
    }
}