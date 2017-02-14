// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedInputType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The type used for arguments
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Linq;

    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Types;

    /// <summary>
    /// The type used for arguments
    /// </summary>
    internal class MergedInputType : MergedObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedInputType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        public MergedInputType(string originalTypeName)
            : base(originalTypeName)
        {
        }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{base.ComplexTypeName}-Input";

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            var fields = this.Fields.Select(this.ConvertApiField);
            var inputGraphType = new VirtualInputGraphType(this.ComplexTypeName);
            inputGraphType.AddFields(fields);
            return inputGraphType;
        }
    }
}
