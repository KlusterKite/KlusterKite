// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedField.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The field in the <see cref="MergedType" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    using ClusterKit.API.Client;

    using global::GraphQL.Resolvers;

    using JetBrains.Annotations;

    /// <summary>
    /// The field in the <see cref="MergedType"/>
    /// </summary>
    internal class MergedField 
    {
        /// <summary>
        /// the list of providers
        /// </summary>
        private readonly List<ApiProvider> providers = new List<ApiProvider>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedField"/> class.
        /// </summary>
        /// <param name="name">
        /// The field name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="provider">
        /// The api provider for the field
        /// </param>
        /// <param name="field">The original field description</param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="arguments">
        /// The field arguments (in case the field is method).
        /// </param>
        /// <param name="description">
        /// The field description
        /// </param>
        public MergedField(
            string name,
            MergedType type,
            ApiProvider provider,
            ApiField field,
            EnFieldFlags flags = EnFieldFlags.None,
            IReadOnlyDictionary<string, MergedField> arguments = null,
            string description = null)
        {
            this.FieldName = name;
            this.Type = type;
            this.Flags = flags;
            this.Arguments = (arguments ?? new Dictionary<string, MergedField>()).ToImmutableDictionary();
            this.Description = description;
            this.AddProvider(provider, field);
        }

        /// <summary>
        /// Gets the type description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the original field name
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets the list of field arguments
        /// </summary>
        [NotNull]
        public IReadOnlyDictionary<string, MergedField> Arguments { get; }

        /// <summary>
        /// Gets the list of the field flags
        /// </summary>
        public EnFieldFlags Flags { get; }

        /// <summary>
        /// Gets the field type
        /// </summary>
        public MergedType Type { get; }

        /// <summary>
        /// Gets or sets the list of providers
        /// </summary>
        public IEnumerable<ApiProvider> Providers => this.providers;

        /// <summary>
        /// Gets the list of original field description by api name
        /// </summary>
        public Dictionary<string, ApiField> OriginalFields { get; } = new Dictionary<string, ApiField>();

        /// <summary>
        /// Gets or sets the resolver to use instead of specified <see cref="MergedType"/>
        /// </summary>
        public IFieldResolver Resolver { get; set; }

        /// <summary>
        /// Adds a provider to the provider list
        /// </summary>
        /// <param name="provider">The provider</param>
        /// <param name="field">The original field description</param>
        public void AddProvider(ApiProvider provider, ApiField field)
        {
            this.providers.Add(provider);
            this.OriginalFields[provider.Description.ApiName] = field;
        }

        /// <summary>
        /// Creates a copy of the current object
        /// </summary>
        /// <returns>The field clone</returns>
        public MergedField Clone()
        {
            var apiProvider = this.providers.First();
            var field = this.OriginalFields[apiProvider.Description.ApiName];

            var mergedField = new MergedField(
                this.FieldName,
                this.Type,
                apiProvider,
                field,
                this.Flags,
                this.Arguments,
                this.Description);

            mergedField.Resolver = this.Resolver;

            foreach (var provider in this.providers.Skip(1))
            {
                mergedField.AddProvider(provider, this.OriginalFields[provider.Description.ApiName]);
            }

            return mergedField;
        }
    }
}
