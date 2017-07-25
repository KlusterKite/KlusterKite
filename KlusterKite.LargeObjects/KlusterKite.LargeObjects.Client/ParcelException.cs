// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelException.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Exception that is thrown in parcel send / receive process
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.LargeObjects.Client
{
    using System;

    /// <summary>
    /// Exception that is thrown in parcel send / receive process
    /// </summary>
    public abstract class ParcelException : Exception
    {
        /// <inheritdoc />
        protected ParcelException()
        {
        }

        /// <inheritdoc />
        protected ParcelException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        protected ParcelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the original notification
        /// </summary>
        public ParcelNotification Notification { get; set; }
    }
}
