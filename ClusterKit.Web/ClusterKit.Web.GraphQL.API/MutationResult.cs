// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MutationResult.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The result of mutation call
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System.Collections.Generic;

    using ClusterKit.Web.GraphQL.Client.Attributes;

    /// <summary>
    /// The result of mutation call
    /// </summary>
    /// <typeparam name="T">The type of mutated object</typeparam>
    public class MutationResult<T>
    {
        /// <summary>
        /// Gets or sets the mutated object
        /// </summary>
        [DeclareField(Description = "The mutated object result")]
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the list of errors
        /// </summary>
        [DeclareField(Description = "The list of mutation errors")]
        public List<ErrorDescription> Errors { get; set; }
    }
}