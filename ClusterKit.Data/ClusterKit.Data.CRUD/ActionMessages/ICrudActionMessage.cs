// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICrudActionMessage.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Request to process some data action
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.ActionMessages
{
    using JetBrains.Annotations;

    /// <summary>
    /// Request to process some data action
    /// </summary>
    public interface ICrudActionMessage
    {
        /// <summary>
        /// Gets the type of request
        /// </summary>
        EnActionType ActionType { get; }

        /// <summary>
        /// Gets the identification of object.
        /// </summary>
        [UsedImplicitly]
        object Id { get; }

        /// <summary>
        /// Gets the object itself.
        /// </summary>
        [UsedImplicitly]
        object Data { get; }

        /// <summary>
        /// Gets some extra data, that will be returned with the response
        /// </summary>
        [UsedImplicitly]
        byte[] ExtraData { get; }
    }
}