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
        /// <param name="isArray">
        /// A value indicating whether this is an array of elements
        /// </param>
        public ApiField(string name, string typeName, bool isArray = false)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.IsScalar = typeName == TypeNameInt || typeName == TypeNameString;
            this.IsArray = isArray;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is an array of elements
        /// </summary>
        public bool IsArray { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field type is a primitive type
        /// </summary>
        public bool IsScalar { get; set; }

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the field type name
        /// </summary>
        public string TypeName { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Name}: {this.TypeName}";
        }
    }
}