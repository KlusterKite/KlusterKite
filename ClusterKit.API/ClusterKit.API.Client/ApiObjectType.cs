// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiObjectType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The api provided type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The api provided type of object with fields
    /// </summary>
    public class ApiObjectType : ApiType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiObjectType"/> class.
        /// </summary>
        public ApiObjectType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiObjectType"/> class.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <param name="fields">
        /// The fields.
        /// </param>
        public ApiObjectType(string typeName, IEnumerable<ApiField> fields = null)
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
        /// Gets or sets the list of direct mutations
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public List<ApiField> DirectMutations { get; set; }

        /// <summary>
        /// Creates a field of this type
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="flags">The list of field flags</param>
        /// <param name="description">The field description</param>
        /// <returns>The api filed</returns>
        public ApiField CreateField(string name, EnFieldFlags flags = EnFieldFlags.Queryable, string description = null)
        {
            return ApiField.Object(name, this.TypeName, flags, description: description);
        }
    }
}