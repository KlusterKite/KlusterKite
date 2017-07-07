// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The merged api type field description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using KlusterKite.API.Client;
    using KlusterKite.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
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
        /// Initializes a new instance of the <see cref="MergedType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        protected MergedType(string originalTypeName)
        {
            this.OriginalTypeName = originalTypeName;
        }

        /// <summary>
        /// Gets combined name from all provider
        /// </summary>
        public abstract string ComplexTypeName { get; }

        /// <summary>
        /// Gets the type description
        /// </summary>
        public virtual string Description => null;

        /// <summary>
        /// Gets the type name of the field as it was first met
        /// </summary>
        public string OriginalTypeName { get; }

        /// <summary>
        /// Removes special symbols from type and field names
        /// </summary>
        /// <param name="name">The original name</param>
        /// <returns>The safe name</returns>
        public static string EscapeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return Regex.Replace(name, "[^a-zA-Z0-9]", "_", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Gets the list of requested fields from parent 
        /// <seealso cref="Field"/>
        /// </summary>
        /// <param name="selectionSet">
        /// The parent field selection set
        /// </param>
        /// <param name="context">
        /// The request context
        /// </param>
        /// <param name="type">
        /// The type containing the fields
        /// </param>
        /// <returns>
        /// The list of fields
        /// </returns>
        public static IEnumerable<Field> GetRequestedFields(
            SelectionSet selectionSet,
            ResolveFieldContext context,
            MergedType type)
        {
            var directFields = selectionSet.Selections.OfType<Field>();
            foreach (var field in directFields)
            {
                yield return field;
            }

            var typeNames = type.GetPossibleFragmentTypeNames().ToList();

            var inlineFragments =
                selectionSet.Selections.OfType<InlineFragment>().Where(f => typeNames.Contains(f.Type.Name));
            foreach (var fragment in inlineFragments)
            {
                foreach (var field in GetRequestedFields(fragment.SelectionSet, context, type))
                {
                    yield return field;
                }
            }

            var fragmentsUsed =
                selectionSet.Selections.OfType<FragmentSpread>()
                    .Select(fs => context.Fragments.FindDefinition(fs.Name))
                    .Where(f => typeNames.Contains(f.Type.Name));

            foreach (var fragment in fragmentsUsed)
            {
                foreach (var field in GetRequestedFields(fragment.SelectionSet, context, type))
                {
                    yield return field;
                }
            }
        }

        /// <summary>
        /// Extracts interface to represent type for specific API provider
        /// </summary>
        /// <param name="provider">
        /// The api provider
        /// </param>
        /// <param name="nodeInterface">
        /// The node interface.
        /// </param>
        /// <returns>
        /// The interface type or null if type is not defined for provider
        /// </returns>
        public abstract IGraphType ExtractInterface(ApiProvider provider, NodeInterface nodeInterface);

        /// <summary>
        /// Gather request parameters
        /// </summary>
        /// <param name="contextFieldAst">
        /// The request context
        /// </param>
        /// <param name="context">
        /// The resolve context.
        /// </param>
        /// <returns>
        /// The list of api requests
        /// </returns>
        public virtual IEnumerable<ApiRequest> GatherSingleApiRequest(
            Field contextFieldAst,
            ResolveFieldContext context)
        {
            yield break;
        }

        /// <summary>
        /// Generates arguments for the field
        /// </summary>
        /// <param name="registeredTypes">
        /// The registered Types.
        /// </param>
        /// <returns>
        /// The generated arguments
        /// </returns>
        public virtual IEnumerable<QueryArgument> GenerateArguments(Dictionary<string, IGraphType> registeredTypes)
        {
            return null;
        }

        /// <summary>
        /// Generates a new <see cref="IGraphType"/>
        /// </summary>
        /// <param name="nodeInterface">
        /// The node Interface.
        /// </param>
        /// <param name="interfaces">
        /// The list of implemented interfaces.
        /// </param>
        /// <returns>
        /// The representing <see cref="IGraphType"/>
        /// </returns>
        public abstract IGraphType GenerateGraphType(NodeInterface nodeInterface, List<TypeInterface> interfaces);

        /// <summary>
        /// Gets the interface name to represent type for specific API provider
        /// </summary>
        /// <param name="provider">The api provider</param>
        /// <returns>The interface type or null if type is not defined for provider</returns>
        public abstract string GetInterfaceName(ApiProvider provider);

        /// <summary>
        /// Gets the list of possible type names used in fragments
        /// </summary>
        /// <returns>The list of type names</returns>
        public virtual IEnumerable<string> GetPossibleFragmentTypeNames()
        {
            yield return this.ComplexTypeName;
        }

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

            return parentData?.GetValue(context.FieldAst.Alias ?? context.FieldName);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ComplexTypeName;
        }
    }
}