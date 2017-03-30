// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MutationException.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The mutation exception
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.Exceptions
{
    using System;
    using System.Collections.Generic;

    using ClusterKit.API.Client;

    /// <summary>
    /// The mutation exception
    /// </summary>
    public class MutationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutationException"/> class.
        /// </summary>
        public MutationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutationException"/> class.
        /// </summary>
        /// <param name="errors">
        /// The list of errors.
        /// </param>
        public MutationException(params ErrorDescription[] errors)
        {
            this.Errors = new List<ErrorDescription>(errors);
        }

        /// <summary>
        /// Gets or sets the list of error descriptions
        /// </summary>
        public List<ErrorDescription> Errors { get; set; }
    }
}
