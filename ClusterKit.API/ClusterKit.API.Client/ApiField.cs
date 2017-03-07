// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiField.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The filed provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The field provider
    /// </summary>
    public class ApiField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiField"/> class.
        /// </summary>
        [UsedImplicitly]
        [Obsolete("Can be used by serializers only", true)]
        public ApiField()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiField"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        protected ApiField(string name, EnFieldFlags flags)
        {
            this.Name = name;
            this.Flags = flags;
        }

        /// <summary>
        /// Gets or sets the list of arguments (if is set - this field becomes a method)
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<ApiField> Arguments { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets the human-readable type description for auto-publishing
        /// </summary>
        [UsedImplicitly]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of defined flags
        /// </summary>
        [UsedImplicitly]
        public EnFieldFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        [UsedImplicitly]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the scalar type of the field
        /// </summary>
        [UsedImplicitly]
        public EnScalarType ScalarType { get; set; }

        /// <summary>
        /// Gets or sets the field type name
        /// </summary>
        [UsedImplicitly]
        public string TypeName { get; set; }

        /// <summary>
        /// Creates an object containing field
        /// </summary>
        /// <param name="name">
        /// The field name
        /// </param>
        /// <param name="typeName">
        /// The field type name
        /// </param>
        /// <param name="flags">
        /// The field flags
        /// </param>
        /// <param name="arguments">
        /// The arguments (if is set - this field becomes a method)
        /// </param>
        /// <param name="description">
        /// The field description
        /// </param>
        /// <returns>
        /// The new field
        /// </returns>
        public static ApiField Object(
            [NotNull] string name,
            [NotNull] string typeName,
            EnFieldFlags flags = EnFieldFlags.None,
            IEnumerable<ApiField> arguments = null,
            string description = null)
        {
            if (flags.HasFlag(EnFieldFlags.IsKey))
            {
                throw new ArgumentException("Object field can't be used as key");
            }

            return new ApiField(name, flags)
                       {
                           TypeName = typeName,
                           ScalarType = EnScalarType.None,
                           Description = description,
                           Arguments =
                               arguments != null
                                   ? new List<ApiField>(arguments)
                                   : new List<ApiField>()
                       };
        }

        /// <summary>
        /// Creates a scalar containing field
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="type">The field type</param>
        /// <param name="flags">The field flags</param>
        /// <param name="arguments">
        /// The arguments (if is set - this field becomes a method)
        /// </param>
        /// <param name="description">
        /// The field description
        /// </param>
        /// <returns>The new field</returns>
        public static ApiField Scalar(
            [NotNull] string name,
            EnScalarType type,
            EnFieldFlags flags = EnFieldFlags.None,
            IEnumerable<ApiField> arguments = null,
            string description = null)
        {
            if (type == EnScalarType.None)
            {
                throw new ArgumentException("Type cannot be None");
            }

            if (flags.HasFlag(EnFieldFlags.IsConnection))
            {
                throw new ArgumentException("Scalar field can't be used as connected objects");
            }

            return new ApiField(name, flags)
                       {
                           ScalarType = type,
                           Description = description,
                           Arguments =
                               arguments != null
                                   ? new List<ApiField>(arguments)
                                   : new List<ApiField>()
                       };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Name}: {this.TypeName}";
        }

        /// <summary>
        /// Creates a clone of the current object
        /// </summary>
        /// <returns>The cloned instance</returns>
        public ApiField Clone()
        {
            return new ApiField(this.Name, this.Flags)
                       {
                           Arguments = new List<ApiField>(this.Arguments),
                           Description = this.Description,
                           ScalarType = this.ScalarType,
                           TypeName = this.TypeName
                       };
        }
    }
}