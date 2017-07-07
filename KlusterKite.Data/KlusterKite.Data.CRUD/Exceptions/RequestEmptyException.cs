// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestEmptyException.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The request was expected to contain data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD.Exceptions
{
    using System;

    /// <summary>
    /// The request was expected to contain data
    /// </summary>
    public class RequestEmptyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestEmptyException"/> class.
        /// </summary>
        public RequestEmptyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestEmptyException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public RequestEmptyException(string message)
            : base(message)
        {
        }
    }
}
