// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VirtualGraphType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The virtual graph type used to convert <see cref="ApiType" /> to <see cref="GraphType" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.GraphTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Publisher.Internals;

    using global::GraphQL.Types;

    /// <summary>
    /// The virtual graph type used to convert <see cref="ApiType"/> to <see cref="GraphType"/>
    /// </summary>
    internal class VirtualGraphType : IObjectGraphType
    {
        /// <summary>
        /// The list of fields
        /// </summary>
        private readonly List<FieldType> fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGraphType"/> class.
        /// </summary>
        /// <param name="virtualTypeName">
        /// The virtual type name.
        /// </param>
        /// <param name="fields">
        /// The list of fields.
        /// </param>
        public VirtualGraphType(string virtualTypeName, List<FieldType> fields = null)
        {
            this.Name = virtualTypeName;
            this.fields = fields ?? new List<FieldType>();
        }

        /// <inheritdoc />
        public string DeprecationReason { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public IEnumerable<FieldType> Fields => this.fields;

        /// <inheritdoc />
        public IEnumerable<Type> Interfaces => new List<Type>();

        /// <inheritdoc />
        public Func<object, bool> IsTypeOf { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Metadata => new Dictionary<string, object>();

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public IEnumerable<IInterfaceGraphType> ResolvedInterfaces => new IInterfaceGraphType[0];

        /// <inheritdoc />
        public FieldType AddField(FieldType fieldType)
        {
            this.fields.Add(fieldType);
            return fieldType;
        }

        /// <summary>
        /// Adds the list of fields to the field list
        /// </summary>
        /// <param name="fieldTypes">The list of fields</param>
        public void AddFields(IEnumerable<FieldType> fieldTypes)
        {
            this.fields.AddRange(fieldTypes);
        }

        /// <inheritdoc />
        public void AddResolvedInterface(IInterfaceGraphType graphType)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public string CollectTypes(TypeCollectionContext context)
        {
            return this.Name;
        }

        /// <inheritdoc />
        public TType GetMetadata<TType>(string key, TType defaultValue = default(TType))
        {
            return defaultValue;
        }

        /// <inheritdoc />
        public bool HasField(string name)
        {
            return this.Fields.Any(f => f.Name == name);
        }

        /// <inheritdoc />
        public bool HasMetadata(string key)
        {
            return false;
        }

        /// <summary>
        /// The same as <see cref="VirtualGraphType"/> but with array container flag
        /// </summary>
        public class Array : VirtualGraphType, IArrayContainerGraph
        {
            /// <inheritdoc />
            public Array(string virtualTypeName, List<FieldType> fields = null)
                : base(virtualTypeName, fields)
            {
            }
        }
    }
}