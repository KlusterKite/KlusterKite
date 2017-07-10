// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The api provided type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using JetBrains.Annotations;

    /// <summary>
    /// The api provided type
    /// </summary>
    public abstract class ApiType
    {
        /// <summary>
        /// Gets or sets the human-readable type description for auto-publishing
        /// </summary>
        [UsedImplicitly]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type name for the api
        /// </summary>
        public string TypeName { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.TypeName;
        }
    }
}