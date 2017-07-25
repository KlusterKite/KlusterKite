// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryConverter.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Converts dictionary to the api list
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client.Converters
{
    using System.Collections.Generic;
    using System.Linq;

    using KlusterKite.API.Attributes;

    /// <summary>
    /// Converts dictionary to the api list
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary key</typeparam>
    /// <typeparam name="TValue">The type of dictionary value</typeparam>
    public class DictionaryConverter<TKey, TValue> : IValueConverter<IEnumerable<Pair<TKey, TValue>>>
    {
        /// <inheritdoc />
        public IEnumerable<Pair<TKey, TValue>> Convert(object source)
        {
            var dictionary = source as IDictionary<TKey, TValue>;
            return dictionary?.Select(p => new Pair<TKey, TValue> { Key = p.Key, Value = p.Value });
        }
    }
}
