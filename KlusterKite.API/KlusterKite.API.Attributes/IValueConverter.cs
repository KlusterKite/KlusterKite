// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IValueConverter.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The interface for value converters
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes
{
    /// <summary>
    /// The interface for value converters
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    public interface IValueConverter<T>
    {
        /// <summary>
        /// Converts the original property/method value to one to be published
        /// </summary>
        /// <param name="source">The original value</param>
        /// <returns>The value to be published</returns>
        T Convert(object source);
    }
}