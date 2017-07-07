// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataUpdaterConfig.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="DataUpdater{TObject}" /> configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using System;

    /// <summary>
    /// The <see cref="DataUpdater{TObject}"/> configuration
    /// </summary>
    internal static class DataUpdaterConfig
    {
        /// <summary>
        /// Gets the list of types to create direct copies
        /// </summary>
        public static readonly Type[] AdditionalScalarTypes =
            {
                typeof(string),
                typeof(decimal),
                typeof(Guid),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(Enum)
            };
    }
}