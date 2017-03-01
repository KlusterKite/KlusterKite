// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The generic provided api description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The generic provided api description
    /// </summary>
    public class ApiDescription : ApiObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescription"/> class.
        /// </summary>
        public ApiDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescription"/> class.
        /// </summary>
        /// <param name="apiName">
        /// The api name.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="types">
        /// The types.
        /// </param>
        /// <param name="fields">
        /// The fields.
        /// </param>
        /// <param name="mutations">
        /// The list of mutations.
        /// </param>
        public ApiDescription(string apiName, string version, IEnumerable<ApiType> types, IEnumerable<ApiField> fields = null, IEnumerable<ApiField> mutations = null)
            : base(apiName, fields)
        {
            this.ApiName = apiName;
            this.Version = Version.Parse(version);
            this.Types.AddRange(types);

            if (mutations != null)
            {
                this.Mutations.AddRange(mutations);
            }
        }

        /// <summary>
        /// Gets or sets the api name
        /// </summary>
        [UsedImplicitly]
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the list of mutation methods
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<ApiField> Mutations { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets the list of registered types
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<ApiType> Types { get; set; } = new List<ApiType>();

        /// <summary>
        /// Gets or sets the api version. Usually this the defining assembly version
        /// </summary>
        [UsedImplicitly]
        public Version Version { get; set; }
    }
}