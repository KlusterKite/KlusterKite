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
        public ApiEnumType(string typeName, IEnumerable<string> values = null)
        {
            this.TypeName = typeName;
            if (values != null)
            {
                this.Values.AddRange(values);
            }
        }

        /// <summary>
        /// Gets or sets the list of enum values
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<string> Values { get; set; } = new List<string>();
    }
}
