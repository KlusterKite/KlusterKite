// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelServerErrorException.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The server could not be contacted or some other error
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.LargeObjects.Client
{
    using System;

    /// <summary>
    /// The server could not be contacted or some other error
    /// </summary>
    public class ParcelServerErrorException : ParcelException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelServerErrorException"/> class.
        /// </summary>
        public ParcelServerErrorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelServerErrorException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public ParcelServerErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelServerErrorException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public ParcelServerErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}