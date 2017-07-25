// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiEnumType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The api provided type of enum value
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The api provided type of enum value
    /// </summary>
    public class ApiEnumType : ApiType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiEnumType"/> class.
        /// </summary>
        public ApiEnumType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiEnumType"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <param name="values">
        /// The list of possible enum values.
        /// </param>
        /// <param name="descriptions">
        /// the list of enum value descriptions
        /// </param>
        public ApiEnumType(
            string typeName,
            IEnumerable<string> values = null,
            IReadOnlyDictionary<string, string> descriptions = null)
        {
            this.TypeName = typeName;
            if (values != null)
            {
                this.Values.AddRange(values);
            }

            if (descriptions != null)
            {
                this.Descriptions = descriptions;
            }
        }

        /// <summary>
        /// Gets or sets the list of enum values
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of enum value descriptions
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public IReadOnlyDictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>();
    }
}
