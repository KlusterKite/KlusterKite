// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryConverter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Converts dictionary to the api list
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Converters
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Client.Attributes;

    /// <summary>
    /// Converts dictionary to the api list
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary key</typeparam>
    /// <typeparam name="TValue">The type of dictionary value</typeparam>
    public class DictionaryConverter<TKey, TValue> : IValueConverter<IEnumerable<DictionaryConverter<TKey, TValue>.Pair>>
    {
        /// <inheritdoc />
        public IEnumerable<Pair> Convert(object source)
        {
            var dictionary = source as IDictionary<TKey, TValue>;
            return dictionary?.Select(p => new Pair { Key = p.Key, Value = p.Value });
        }

        /// <summary>
        /// Represents a dictionary key-value pair
        /// </summary>
        [ApiDescription(Description = "Represents a dictionary key-value pair")]
        public class Pair
        {
            /// <summary>
            /// Gets or sets a dictionary key
            /// </summary>
            [DeclareField(Description = "Gets a dictionary key")]
            public TKey Key { get; set; }

            /// <summary>
            /// Gets or sets a dictionary value
            /// </summary>
            [DeclareField(Description = "Gets a dictionary value")]
            public TValue Value { get; set; }
        }
    }
}
