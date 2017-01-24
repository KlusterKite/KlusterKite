// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityNotFoundException.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Entity was not found in the datasource
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.Exceptions
{
    using System;

    /// <summary>
    /// Entity was not found in the datasource
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        public EntityNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public EntityNotFoundException(string message)
            : base(message)
        {
        }
    }
}