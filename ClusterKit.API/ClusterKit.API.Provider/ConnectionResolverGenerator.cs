// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generates the <see cref="ConnectionResolver" /> code
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Text.RegularExpressions;

    using ClusterKit.API.Provider.Resolvers;

    /// <summary>
    /// Generates the <see cref="ConnectionResolver"/> code
    /// </summary>
    internal class ConnectionResolverGenerator : ResolverGenerator
    {
        /// <inheritdoc />
        public ConnectionResolverGenerator(Type objectType, AssembleTempData data)
            : base(objectType, data)
        {
        }

        /// <inheritdoc />
        protected override string ClassName => "ConnectionResolver"
                                               + $"_{Regex.Replace(ToCSharpRepresentation(this.ObjectType), "[^a-zA-Z0-9]", string.Empty)}"
                                               + $"_{this.Uid:N}";

        /// <inheritdoc />
        public override string Generate()
        {
            return string.Empty;
        }
    }
}
