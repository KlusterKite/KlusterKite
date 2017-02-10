// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The generic provided api description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The generic provided api description
    /// </summary>
    public class ApiDescription : ApiType
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
        /// <param name="methods">
        /// The methods.
        /// </param>
        public ApiDescription(string apiName, string version, IEnumerable<ApiType> types, IEnumerable<ApiField> fields = null, IEnumerable<ApiMethod> methods = null)
            : base(apiName, fields, methods)
        {
            this.ApiName = apiName;
            this.Version = Version.Parse(version);
            this.Types.AddRange(types);
        }

        /// <summary>
        /// Gets or sets the api name
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the list of registered types
        /// </summary>
        [NotNull]
        public List<ApiType> Types { get; set; } = new List<ApiType>();

        /// <summary>
        /// Gets or sets the api version. Usually this the defining assembly version
        /// </summary>
        public Version Version { get; set; }
    }
}