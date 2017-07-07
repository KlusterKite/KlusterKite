// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageWithExtraData.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Message contains additional data, that should be returned with response (a was provided in request)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD
{
    using JetBrains.Annotations;

    /// <summary>
    /// Message contains additional data, that should be returned with response (a was provided in request)
    /// </summary>
    public interface IMessageWithExtraData
    {
        /// <summary>
        /// Gets or sets some extra data, that will be returned with the response
        /// </summary>
        [UsedImplicitly]
        byte[] ExtraData { get; set; }
    }
}