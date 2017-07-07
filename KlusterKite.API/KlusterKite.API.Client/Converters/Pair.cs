// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Pair.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Represents a dictionary key-value pair
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client.Converters
{
    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;

    /// <summary>
    /// Represents a dictionary key-value pair
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of value
    /// </typeparam>
    [ApiDescription(Description = "Represents a dictionary key-value pair", Name = "Pair")]
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