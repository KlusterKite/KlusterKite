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
    /// <summary>
    /// The root query type
    /// </summary>
    internal class MergedRoot : MergedObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedRoot"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        public MergedRoot(string originalTypeName)
            : base(originalTypeName)
        {
        }

        /// <inheritdoc />
        public override string Description => "The root query type";
    }
}
