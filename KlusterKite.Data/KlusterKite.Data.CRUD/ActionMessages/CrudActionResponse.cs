// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrudActionResponse.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Standard response from sthe <seealso cref="RestActionMessage{TData,TId}" /> request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD.ActionMessages
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Standard response from the <seealso cref="CrudActionMessage{TData,TId}"/> request
    /// </summary>
    /// <typeparam name="TData">The type of entity</typeparam>
    public class CrudActionResponse<TData> : IMessageWithExtraData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrudActionResponse{TData}"/> class.
        /// </summary>
        [UsedImplicitly]
        public CrudActionResponse()
        {
        }

        /// <summary>
        /// Gets or sets the object itself.
        /// </summary>
        [UsedImplicitly]
        public TData Data { get; set; }

        /// <summary>
        /// Gets or sets the exception data in case of failure
        /// </summary>
        [UsedImplicitly]
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets some extra data, that was sent with the request
        /// </summary>
        [UsedImplicitly]
        public byte[] ExtraData { get; set; }

        /// <summary>
        /// Creates a failed response
        /// </summary>
        /// <param name="exception">The failure exception</param>
        /// <param name="extraData">Some extra data, that was sent with the request</param>
        /// <returns>The new response</returns>
        public static CrudActionResponse<TData> Error(Exception exception, byte[] extraData)
        {
            return new CrudActionResponse<TData>
            {
                Exception = exception,
                ExtraData = extraData
            };
        }

        /// <summary>
        /// Creates a success response
        /// </summary>
        /// <param name="data">The actual entity data</param>
        /// <param name="extraData">Some extra data, that was sent with the request</param>
        /// <returns>The new response</returns>
        public static CrudActionResponse<TData> Success(TData data, byte[] extraData)
        {
            return new CrudActionResponse<TData>
            {
                Data = data,
                ExtraData = extraData
            };
        }
    }
}