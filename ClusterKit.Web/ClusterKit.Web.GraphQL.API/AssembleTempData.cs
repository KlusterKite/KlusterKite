// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssembleTempData.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The temporary data used during assemble process
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using ClusterKit.Web.GraphQL.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// The temporary data used during assemble process
    /// </summary>
    internal class AssembleTempData
    {
        /// <summary>
        /// Gets the list of descriptions of discovered types used in API
        /// </summary>
        [NotNull]
        public Dictionary<string, ApiType> DiscoveredApiTypes { get; } = new Dictionary<string, ApiType>();

        /// <summary>
        /// Gets the list of descriptions of described types by original type full names
        /// </summary>
        [NotNull]
        public Dictionary<string, ApiType> ApiTypeByOriginalTypeNames { get; } = new Dictionary<string, ApiType>();

        /// <summary>
        /// Gets the list of descriptions of discovered types used in API
        /// </summary>
        [NotNull]
        public Dictionary<string, Type> DiscoveredTypes { get; } = new Dictionary<string, Type>();

        /// <summary>
        /// Gets the list of field names by fields
        /// </summary>
        [NotNull]
        public Dictionary<MemberInfo, string> FieldNames { get; } = new Dictionary<MemberInfo, string>();

        /// <summary>
        /// Gets the list of resolver class names for the field in the type
        /// </summary>
        [NotNull]
        public Dictionary<string, Dictionary<string, string>> ResolverNames { get; } = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Gets the list of resolver class names for mutations
        /// </summary>
        [NotNull]
        public Dictionary<string, string> MutationResolverNames { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the list of properties described in API
        /// </summary>
        [NotNull]
        public Dictionary<string, Dictionary<string, MemberInfo>> Members { get; } = new Dictionary<string, Dictionary<string, MemberInfo>>();

        /// <summary>
        /// Gets the list of resolver generators
        /// </summary>
        public List<ResolverGenerator> ResolverGenerators { get; } = new List<ResolverGenerator>();
    }
}