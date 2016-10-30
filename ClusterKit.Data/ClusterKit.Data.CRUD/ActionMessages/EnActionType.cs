// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnActionType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the RestActionEnum type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.ActionMessages
{
    using JetBrains.Annotations;

    /// <summary>
    /// Types of object actions
    /// </summary>
    [UsedImplicitly]
    public enum EnActionType
    {
        /// <summary>
        /// Get an object from data source
        /// </summary>
        [UsedImplicitly]
        Get = 1,

        /// <summary>
        /// Creates an object in data source
        /// </summary>
        [UsedImplicitly]
        Create = 2,

        /// <summary>
        /// Creates or updates an object in data source
        /// </summary>
        [UsedImplicitly]
        Update = 3,

        /// <summary>
        /// Removes an object from data source
        /// </summary>
        [UsedImplicitly]
        Delete = 4
    }
}