// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringConverter.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The basic converter that returns <see cref="object.ToString" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client.Converters
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The basic converter that returns <see cref="object.ToString"/>
    /// </summary>
    public class StringConverter : IValueConverter<string>
    {
        /// <inheritdoc />
        public string Convert(object source)
        {
            return source?.ToString();
        }
    }
}
