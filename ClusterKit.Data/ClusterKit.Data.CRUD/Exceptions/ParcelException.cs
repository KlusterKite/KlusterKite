// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelException.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Parcel decomposition exception
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.Exceptions
{
    using System;

    /// <summary>
    /// Parcel decomposition exception
    /// </summary>
    public class ParcelException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelException"/> class.
        /// </summary>
        public ParcelException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public ParcelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public ParcelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
