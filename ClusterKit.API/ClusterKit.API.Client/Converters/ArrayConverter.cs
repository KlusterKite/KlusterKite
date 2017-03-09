// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayConverter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic converter for arrays
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Converters
{
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    /// <summary>
    /// Generic converter for arrays
    /// </summary>
    /// <typeparam name="TConverter">The type of element converter</typeparam>
    /// <typeparam name="TObject">The type of element</typeparam>
    [UsedImplicitly]
    public class ArrayConverter<TConverter, TObject> : IValueConverter<IEnumerable<TObject>>
        where TConverter : IValueConverter<TObject>, new()
    {
        /// <inheritdoc />
        public IEnumerable<TObject> Convert(object source)
        {
            var typedSource = source as IEnumerable<object>;
            if (typedSource == null)
            {
                return null;
            }

            var converter = new TConverter();
            return typedSource.Select(v => converter.Convert(v));
        }
    }
}