// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Pair.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Represents a dictionary key-value pair
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Converters
{
    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Represents a dictionary key-value pair
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of value
    /// </typeparam>
    [ApiDescription(Description = "Represents a dictionary key-value pair")]
    public class Pair<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets a dictionary key
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "Gets a dictionary key", IsKey = true)]
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets a dictionary value
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "Gets a dictionary value")]
        public TValue Value { get; set; }
    }
}