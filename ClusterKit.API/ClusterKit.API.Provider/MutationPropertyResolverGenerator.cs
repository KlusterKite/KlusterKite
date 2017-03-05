// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MutationPropertyResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Initializes a new instance of the <see cref="PropertyResolverGenerator" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Creates resolver for the mutations
    /// </summary>
    internal class MutationPropertyResolverGenerator : PropertyResolverGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutationPropertyResolverGenerator"/> class.
        /// </summary>
        /// <param name="fieldsPath">
        /// The fields Path.
        /// </param>
        /// <param name="typesPath">
        /// The types Path.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="metadata">
        /// The return type metadata.
        /// </param>
        /// <param name="sourceType">
        /// The source type.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public MutationPropertyResolverGenerator(
            List<string> fieldsPath,
            List<string> typesPath,
            MethodInfo member, 
            TypeMetadata metadata, 
            Type sourceType, 
            AssembleTempData data)
            : base(member, metadata, sourceType, data)
        {
            this.FieldsPath = fieldsPath;
            this.TypesPath = typesPath;
        }

        /// <inheritdoc />
        protected override bool GetValueIsAsync => true; 

        /// <summary>
        /// Gets the fields path to the mutation
        /// </summary>
        private List<string> FieldsPath { get; }

        /// <summary>
        /// Gets the types path to the mutation
        /// </summary>
        private List<string> TypesPath { get; }

        /// <inheritdoc />
        public override string Generate()
        {
            var generate = base.Generate();
            return generate;
        }

        /// <inheritdoc />
        protected override string GenerateResultSourceAcquirement()
        {
            return string.Empty;
            /*
            if (this.FieldsPath.Count != this.TypesPath.Count)
            {
                throw new Exception("FieldsPath and TypesPath lengths should be equal");
            }

            var valueRetrievals = new List<string>
                                      {
                                          "var source0 = source;"
                                      };
            for (var i = 0; i < this.FieldsPath.Count; i++)
            {
                var fieldName = this.FieldsPath[i];
                var apiTypeName = this.TypesPath[i];
                var command = $@"
                    var source{i+1} = await new {this.Data.ResolverNames[apiTypeName][fieldName]}().GetValue(source{i}, new ApiRequest {{ FieldName = ""{fieldName}"" }}, context, argumentsSerializer);
                ";
                valueRetrievals.Add(command);
            }

            return $@"
                {string.Join("\n", valueRetrievals)}
                source = source{this.FieldsPath.Count};
                {base.GenerateResultSourceAcquirement()}";
                */
        }
    }
}
