// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper, that generates C# code for the resolvers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;

    /// <summary>
    /// The base class for resolver generators
    /// </summary>
    internal abstract class ResolverGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolverGenerator"/> class.
        /// </summary>
        /// <param name="objectType">
        /// The object to generate resolver for
        /// </param>
        /// <param name="data">
        /// The global assembled data.
        /// </param>
        protected ResolverGenerator(Type objectType, AssembleTempData data)
        {
            this.ObjectType = objectType;
            this.Data = data;
        }

        /// <summary>
        /// Gets the generated class name
        /// </summary>
        public string FullClassName => $"ClusterKit.API.Provider.Dynamic.{this.ClassName}";

        /// <summary>
        /// Gets the generated class name
        /// </summary>
        protected abstract string ClassName { get; }

        /// <summary>
        /// Gets the global assembled data
        /// </summary>
        protected AssembleTempData Data { get; }

        /// <summary>
        /// Gets the type of object to generate resolver for
        /// </summary>
        protected Type ObjectType { get; }

        /// <summary>
        /// Gets the resolver uid
        /// </summary>
        protected Guid Uid { get; } = Guid.NewGuid();

        /// <summary>
        /// Creates c# code for defined parameters
        /// </summary>
        /// <returns>The field resolver definition in C#</returns>
        public abstract string Generate();
    }
}