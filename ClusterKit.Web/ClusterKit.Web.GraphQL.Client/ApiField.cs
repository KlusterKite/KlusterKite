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
        /// The reserved type name for integer primitive
        /// </summary>
        public const string TypeNameInt = "int";

        /// <summary>
        /// The reserved type name for string primitive
        /// </summary>
        public const string TypeNameString = "string";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiField"/> class.
        /// </summary>
        [UsedImplicitly]
        public ApiField()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiField"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <param name="flags">
        /// The list of field flags
        /// </param>
        public ApiField(string name, string typeName, EnFlags flags = EnFlags.None)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.Flags = flags;
            if (typeName == TypeNameInt || typeName == TypeNameString)
            {
                this.Flags |= EnFlags.IsScalar;
            }
        }

        /// <summary>
        /// The list of field flags
        /// </summary>
        [Flags]
        public enum EnFlags
        {
            /// <summary>
            /// No special flags were set
            /// </summary>
            None = 0,

            /// <summary>
            /// Field type is a primitive type
            /// </summary>
            IsScalar = 1,

            /// <summary>
            /// This is an array of elements
            /// </summary>
            IsArray = 2,

            /// <summary>
            /// The field is an object key
            /// </summary>
            IsKey = 5
        }

        /// <summary>
        /// Gets or sets the list of defined flags
        /// </summary>
        [UsedImplicitly]
        public EnFlags Flags { get; set; }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Name}: {this.TypeName}";
        }
    }
}