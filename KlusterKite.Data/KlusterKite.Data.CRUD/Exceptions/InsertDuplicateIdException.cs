// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertDuplicateIdException.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the InsertDuplicateIdException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD.Exceptions
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Thrown on insert operation, in case the object with the same id is already in the datasource
    /// </summary>
    [UsedImplicitly]
    public class InsertDuplicateIdException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertDuplicateIdException"/> class.
        /// </summary>
        public InsertDuplicateIdException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertDuplicateIdException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public InsertDuplicateIdException(string message)
            : base(message)
        {
        }
    }
}
