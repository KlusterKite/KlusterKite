// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BeforeActionException.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Action was aborted by "Before" processor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD.Exceptions
{
    using System;

    /// <summary>
    /// Action was aborted by "Before" processor
    /// </summary>
    public class BeforeActionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeActionException"/> class.
        /// </summary>
        public BeforeActionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeActionException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public BeforeActionException(string message)
            : base(message)
        {
        }
    }
}
