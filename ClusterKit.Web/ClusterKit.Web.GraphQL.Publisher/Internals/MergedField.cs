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

    using ClusterKit.Web.GraphQL.Client;

    using JetBrains.Annotations;

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
        /// <param name="arguments">
        /// The field arguments (in case the field is method).
        /// </param>
        public MergedField(MergedType type, EnFieldFlags flags = EnFieldFlags.None, Dictionary<string, MergedField> arguments = null)
        {
            this.Type = type;
            this.Flags = flags;
            this.Arguments = (arguments ?? new Dictionary<string, MergedField>()).ToImmutableDictionary();
        }

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
    }
}
