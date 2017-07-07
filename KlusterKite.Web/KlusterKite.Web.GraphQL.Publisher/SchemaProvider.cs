// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchemaProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Singleton used to provide <see cref="Schema" /> from <see cref="ApiBrowserActor" /> to <see cref="EndpointController" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher
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
