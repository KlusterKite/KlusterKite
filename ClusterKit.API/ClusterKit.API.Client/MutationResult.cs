// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MutationResult.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The result of mutation call
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System.Collections.Generic;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The result of mutation call
    /// </summary>
    /// <typeparam name="T">The type of mutated object</typeparam>
    [ApiDescription("The standard mutation result", Name = "MutationResult")]
    public class MutationResult<T>
    {
        /// <summary>
        /// the list of errors
        /// </summary>
        private List<ErrorDescription> errors;

        /// <summary>
        /// Gets or sets the mutated object
        /// </summary>
        [DeclareField("The mutated object result")]
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the list of errors
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The list of mutation errors")]
        public List<ErrorDescription> Errors
        {
            get
            {
                return this.errors;
            }

            set
            {
                var errorList = new List<ErrorDescription>();
                if (value == null)
                {
                    return;
                }

                foreach (var errorDescription in value)
                {
                    errorDescription.Number = errorList.Count + 1;
                    errorList.Add(errorDescription);
                }

                this.errors = errorList;
            }
        }
    }
}