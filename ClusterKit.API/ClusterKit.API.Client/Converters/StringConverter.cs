// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringConverter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The basic converter that returns <see cref="object.ToString" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Converters
{
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
