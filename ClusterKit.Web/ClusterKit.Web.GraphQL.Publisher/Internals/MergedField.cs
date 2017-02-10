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
    using ClusterKit.Web.GraphQL.Client;

    /// <summary>
    /// The field in the <see cref="MergedType"/>
    /// </summary>
    internal class MergedField 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedField"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        public MergedField(MergedType type, ApiField.EnFlags flags = ApiField.EnFlags.None)
        {
            this.Type = type;
            this.Flags = flags;
        }

        /// <summary>
        /// Gets or sets the list of the field flags
        /// </summary>
        public ApiField.EnFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the field type
        /// </summary>
        public MergedType Type { get; set; }
    }
}
