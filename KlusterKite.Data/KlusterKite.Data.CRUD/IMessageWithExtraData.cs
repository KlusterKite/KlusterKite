// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageWithExtraData.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Message contains additional data, that should be returned with response (a was provided in request)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD
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