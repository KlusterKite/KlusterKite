// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged api type field description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;

    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merged api abstract type description
    /// </summary>
    internal abstract class MergedType : IFieldResolver
    {
        /// <summary>
        /// The metadata key to access the original <see cref="MergedType"/> object
        /// </summary>
        public const string MetaDataTypeKey = "MergedType";

        /// <summary>
        /// The metadata key to access the original <see cref="MergedType"/> object
        /// </summary>
        public const string MetaDataFlagsKey = "Flags";

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        public MergedType(string originalTypeName)
        {
            this.OriginalTypeName = originalTypeName;
        }

        /// <summary>
        /// Gets the type name of the field as it was first met
        /// </summary>
        public string OriginalTypeName { get; }

        /// <summary>
        /// Gets combined name from all provider
        /// </summary>
        public abstract string ComplexTypeName { get; }

        /// <summary>
        /// Gets the list of providers
        /// </summary>
        public abstract IEnumerable<FieldProvider> Providers { get;  }

        /// <summary>
        /// Resolves request value
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>Resolved value</returns>
        public virtual object Resolve(ResolveFieldContext context)
        {
            var parentData = context.Source as JObject;
            if (context.ParentType is IArrayContainerGraph && parentData?.Parent?.Type == JTokenType.Array)
            {
                return parentData;
            }

            return parentData?.GetValue(context.FieldName);
        }

        /// <summary>
        /// Get all included types recursively
        /// </summary>
        /// <returns>The list of all defined types</returns>
        public abstract IEnumerable<MergedType> GetAllTypes();

        /// <summary>
        /// Generates a new <see cref="IGraphType"/>
        /// </summary>
        /// <returns>The representing <see cref="IGraphType"/></returns>
        public abstract IGraphType GenerateGraphType();

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ComplexTypeName;
        }

        /// <summary>
        /// Generates arguments for the field
        /// </summary>
        /// <returns>The generated arguments</returns>
        public virtual QueryArguments GenerateArguments()
        {
            return null;
        }
    }
}