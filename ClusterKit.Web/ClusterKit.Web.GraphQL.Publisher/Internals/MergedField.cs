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
            EnFieldFlags flags = EnFieldFlags.None,
            IReadOnlyDictionary<string, MergedField> arguments = null,
            string description = null)
        {
            this.FieldName = name;
            this.Type = type;
            this.Flags = flags;
            this.Arguments = (arguments ?? new Dictionary<string, MergedField>()).ToImmutableDictionary();
            this.Description = description;
            this.providers.Add(provider);
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
        /// Adds a provider to the provider list
        /// </summary>
        /// <param name="provider">The provider</param>
        public void AddProvider(ApiProvider provider)
        {
            this.providers.Add(provider);
        }

        /// <summary>
        /// Adds the list of providers to the provider list
        /// </summary>
        /// <param name="newProviders">The list of providers</param>
        public void AddProviders(IEnumerable<ApiProvider> newProviders)
        {
            this.providers.AddRange(newProviders);
        }

        /// <summary>
        /// Creates a copy of the current object
        /// </summary>
        /// <returns>The field clone</returns>
        public MergedField Clone()
        {
            var mergedField = new MergedField(
                this.FieldName,
                this.Type,
                this.providers.First(),
                this.Flags,
                this.Arguments,
                this.Description);
            mergedField.providers.AddRange(this.providers.Skip(1));
            return mergedField;
        }
    }
}
