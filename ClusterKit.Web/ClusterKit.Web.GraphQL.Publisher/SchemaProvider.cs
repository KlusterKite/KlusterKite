// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchemaProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Singleton used to provide <see cref="Schema" /> from <see cref="ApiBrowserActor" /> to <see cref="EndpointController" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using global::GraphQL.Types;

    /// <summary>
    /// Singleton used to provide <see cref="Schema"/> from <see cref="ApiBrowserActor"/> to <see cref="EndpointController"/>
    /// </summary>
    public class SchemaProvider
    {
        /// <summary>
        /// Gets or sets the current schema
        /// </summary>
        public Schema CurrentSchema { get; set; }
    }
}
