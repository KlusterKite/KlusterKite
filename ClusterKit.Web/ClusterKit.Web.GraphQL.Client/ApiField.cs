// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiField.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The filed provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// The filed provider
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
        private ApiField(string name, EnFieldFlags flags)
        {
            this.Name = name;
            this.Flags = flags;
        }

        /// <summary>
        /// Gets or sets the list of defined flags
        /// </summary>
        [UsedImplicitly]
        public EnFieldFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the scalar type of the field
        /// </summary>
        public EnScalarType ScalarType { get; set; }

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        [UsedImplicitly]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the field type name
        /// </summary>
        [UsedImplicitly]
        public string TypeName { get; set; }

        /// <summary>
        /// Creates an object containing field
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="typeName">The field type name</param>
        /// <param name="flags">The field flags</param>
        /// <returns>The new field</returns>
        public static ApiField Object([NotNull]string name, [NotNull]string typeName, EnFieldFlags flags = EnFieldFlags.None)
        {
            if (flags.HasFlag(EnFieldFlags.IsKey))
            {
                throw new ArgumentException("Object field can't be used as key");
            }

            return new ApiField(name, flags) { TypeName = typeName, ScalarType = EnScalarType.None }; 
        }

        /// <summary>
        /// Creates a scalar containing field
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="type">The field type</param>
        /// <param name="flags">The field flags</param>
        /// <returns>The new field</returns>
        public static ApiField Scalar([NotNull] string name, EnScalarType type, EnFieldFlags flags = EnFieldFlags.None)
        {
            if (type == EnScalarType.None)
            {
                throw new ArgumentException("Type cannot be None");
            }

            if (flags.HasFlag(EnFieldFlags.IsConnection))
            {
                throw new ArgumentException("Scalar field can't be used as connected objects");
            }

            return new ApiField(name, flags) { ScalarType = type };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Name}: {this.TypeName}";
        }
    }
}