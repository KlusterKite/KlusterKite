// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The generic provided api description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// TODO: remove next comment
// ReSharper disable StyleCop.SA1402

namespace ClusterKit.Web.GraphQL.Client
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The generic provided api description
    /// </summary>
    public class ApiDescription : ApiType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescription"/> class.
        /// </summary>
        public ApiDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescription"/> class.
        /// </summary>
        /// <param name="apiName">
        /// The api name.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="types">
        /// The types.
        /// </param>
        /// <param name="fields">
        /// The fields.
        /// </param>
        /// <param name="methods">
        /// The methods.
        /// </param>
        public ApiDescription(string apiName, string version, IEnumerable<ApiType> types, IEnumerable<ApiField> fields = null, IEnumerable<ApiMethod> methods = null)
            : base(apiName, fields, methods)
        {
            this.ApiName = apiName;
            this.Version = Version.Parse(version);
            this.Types.AddRange(types);
        }

        /// <summary>
        /// Gets or sets the api name
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the list of registered types
        /// </summary>
        [NotNull]
        public List<ApiType> Types { get; set; } = new List<ApiType>();

        /// <summary>
        /// Gets or sets the api version. Usually this the defining assembly version
        /// </summary>
        public Version Version { get; set; }
    }

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
    }

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
        /// <param name="methods">
        /// The methods.
        /// </param>
        public ApiType(string typeName, IEnumerable<ApiField> fields = null,  IEnumerable<ApiMethod> methods = null)
        {
            this.TypeName = typeName;
            if (fields != null)
            {
                this.Fields.AddRange(fields);
            }

            if (methods != null)
            {
                this.Methods.AddRange(methods);
            }
        }

        /// <summary>
        /// Gets or sets the list of fields
        /// </summary>
        [NotNull]
        public List<ApiField> Fields { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets the list of methods
        /// </summary>
        [NotNull]
        public List<ApiMethod> Methods { get; set; } = new List<ApiMethod>();

        /// <summary>
        /// Gets or sets the type name for the api
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Creates a field of this type
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="isArray">A value indicating whether this is an array of elements</param>
        /// <returns>The api filed</returns>
        public ApiField CreateField(string name, bool isArray = false)
        {
            return new ApiField(name, this.TypeName, isArray);
        }
    }

    /// <summary>
    /// The published type method
    /// </summary>
    public class ApiMethod
    {
        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the method parameters list
        /// </summary>
        public List<ApiField> Parameters { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets a value indicating whether the return value is an array of elements
        /// </summary>
        public bool ReturnsArray { get; set; }

        /// <summary>
        /// Gets or sets the method return type name
        /// </summary>
        public string ReturnType { get; set; }
    }
}