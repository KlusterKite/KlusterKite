// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The api provided type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The api provided type
    /// </summary>
    public class ApiType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiType"/> class.
        /// </summary>
        public ApiType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiType"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <param name="fields">
        /// The fields.
        /// </param>
        public ApiType(string typeName, IEnumerable<ApiField> fields = null)
        {
            this.TypeName = typeName;
            if (fields != null)
            {
                this.Fields.AddRange(fields);
            }
        }

        /// <summary>
        /// Gets or sets the list of fields
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<ApiField> Fields { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets the type name for the api
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Creates a field of this type
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="flags">The list of field flags</param>
        /// <returns>The api filed</returns>
        public ApiField CreateField(string name, EnFieldFlags flags = EnFieldFlags.None)
        {
            return ApiField.Object(name, this.TypeName, flags);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.TypeName;
        }
    }
}