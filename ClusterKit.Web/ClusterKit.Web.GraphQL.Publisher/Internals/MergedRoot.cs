// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedRoot.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The root query type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;

    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Types;

    /// <summary>
    /// The root query type
    /// </summary>
    internal class MergedRoot : MergedObjectType
    {
        /// <summary>
        /// The node searcher
        /// </summary>
        private NodeSearcher searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedRoot"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        /// <param name="providers">
        /// The providers.
        /// </param>
        public MergedRoot(string originalTypeName, List<ApiProvider> providers)
            : base(originalTypeName)
        {
            this.searcher = new NodeSearcher(providers);
        }

        /// <inheritdoc />
        public override string Description => "The root query type";

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            var graphType = (VirtualGraphType)base.GenerateGraphType();
            var nodeFieldType = new FieldType();
            nodeFieldType.Name = "node";
            nodeFieldType.ResolvedType = new NodeInterface();
            nodeFieldType.Description = "The node global searcher according to Relay specification";
            nodeFieldType.Arguments =
                new QueryArguments(
                    new QueryArgument(typeof(IdGraphType)) { Name = "id", Description = "The node global id" });
            nodeFieldType.Resolver = this.searcher;
            graphType.AddField(nodeFieldType);

            return graphType;
        }
    }
}
