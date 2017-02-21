// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper, that generates C# code for the resolvers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Client.Attributes;

    /// <summary>
    /// Helper, that generates C# code for the resolvers
    /// </summary>
    /// <remarks>
    /// Resolver gets the type field value for the request. So for each published property in each published type we should have personal resolver
    /// </remarks>
    internal class ResolverGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolverGenerator"/> class.
        /// </summary>
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
        public ResolverGenerator(MemberInfo member, TypeMetadata metadata, Type sourceType, AssembleTempData data)
        {
            this.Member = member;
            this.Metadata = metadata;
            this.SourceType = sourceType;
            this.Data = data;
        }

        /// <summary>
        /// Gets the generated class name
        /// </summary>
        public string ClassName => $"ClusterKit.Web.GraphQL.Dynamic.Resolver{this.Uid:N}";

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> representing field
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the return type metadata
        /// </summary>
        public TypeMetadata Metadata { get; }

        /// <summary>
        /// Gets the source data type
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets the assemble data
        /// </summary>
        public AssembleTempData Data { get; }

        /// <summary>
        /// Gets the resolver uid
        /// </summary>
        public Guid Uid { get; } = Guid.NewGuid();

        /// <summary>
        /// Creates c# code for defined parameters
        /// </summary>
        /// <returns>The field resolver definition in C#</returns>
        public string Generate()
        {
            var @async = this.Metadata.IsAsync || this.Metadata.ScalarType == EnScalarType.None ? "async" : string.Empty;
            var code = $@"
                    namespace ClusterKit.Web.GraphQL.Dynamic {{
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        using System.Linq;
                        using System.Linq.Expressions;

                        using Newtonsoft.Json;
                        using Newtonsoft.Json.Linq;
                    
                        using ClusterKit.Security.Client;
                        using ClusterKit.Web.GraphQL.Client;
                        using ClusterKit.Web.GraphQL.API.Resolvers;

                        public class Resolver{this.Uid:N} : PropertyResolver {{
                            public override {@async} Task<JToken> Resolve(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer) {{
                                var sourceTyped = source as {ToCSharpRepresentation(this.SourceType, true)};
                                if (sourceTyped == null) {{
                                    // todo: report error
                                    return null;
                                }}

                                {this.GenerateResultSourceAcquirement()};
                                {(this.Metadata.IsForwarding ? this.GenerateForwardedReturn() : this.GenerateRecursiveResolve())}
                            }}
                        }}
                    }}
                ";

            return code;
        }

        /// <summary>
        /// Converts string to camel case
        /// </summary>
        /// <param name="name">The property / method name</param>
        /// <returns>The name in camel case</returns>
        internal static string ToCamelCase(string name)
        {
            name = name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var array = name.ToCharArray();
            array[0] = array[0].ToString().ToLowerInvariant().ToCharArray().First();
            return new string(array);
        }

        /// <summary>
        /// Creates valid type name representation
        /// </summary>
        /// <remarks>
        /// Original code: <see href="http://stackoverflow.com/questions/2579734/get-the-type-name"/>
        /// </remarks>
        /// <param name="type">The type</param>
        /// <param name="trimArgCount">A value indicating whether to trim arguments count</param>
        /// <returns>A valid C# name</returns>
        private static string ToCSharpRepresentation(Type type, bool trimArgCount)
        {
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments().ToList();
                return ToCSharpRepresentation(type, trimArgCount, genericArgs);
            }

            return type.FullName.Replace("+", ".");
        }

        /// <summary>
        /// Creates valid type name representation
        /// </summary>
        /// <remarks>
        /// Original code: <see href="http://stackoverflow.com/questions/2579734/get-the-type-name"/>
        /// </remarks>
        /// <param name="type">The type</param>
        /// <param name="trimArgCount">A value indicating whether to trim arguments count</param>
        /// <param name="availableArguments">The list of type parameters for generic classes</param>
        /// <returns>A valid C# name</returns>
        private static string ToCSharpRepresentation(Type type, bool trimArgCount, List<Type> availableArguments)
        {
            if (!type.IsGenericType)
            {
                return type.FullName.Replace("+", ".");
            }

            var value = type.FullName.Replace("+", ".");
            if (trimArgCount && value.IndexOf("`", StringComparison.InvariantCulture) > -1)
            {
                value = value.Substring(0, value.IndexOf("`", StringComparison.InvariantCulture));
            }

            if (type.DeclaringType != null)
            {
                // This is a nested type, build the nesting type first
                value = ToCSharpRepresentation(type.DeclaringType, trimArgCount, availableArguments) + "+" + value;
            }

            // Build the type arguments (if any)
            var argString = string.Empty;
            var thisTypeArgs = type.GetGenericArguments();
            for (var i = 0; i < thisTypeArgs.Length && availableArguments.Count > 0; i++)
            {
                if (i != 0)
                {
                    argString += ", ";
                }

                argString += ToCSharpRepresentation(availableArguments[0], trimArgCount);
                availableArguments.RemoveAt(0);
            }

            // If there are type arguments, add them with < >
            if (argString.Length > 0)
            {
                value += "<" + argString + ">";
            }

            return value;
        }

        /// <summary>
        /// Generates the execution of property / method of source value
        /// </summary>
        /// <returns>Code to acquire the source value</returns>
        private string GenerateResultSourceAcquirement()
        {
            var property = this.Member as PropertyInfo;
            if (property != null)
            {
                return this.GenerateResultSourceFromPropertyAcquirement(property);
            }

            var method = this.Member as MethodInfo;
            if (method != null)
            {
                return this.GenerateResultSourceFromMethodAcquirement(method);
            }

            // todo: method execution
            throw new InvalidOperationException($"Member is of the invalid type {this.Member.GetType().Name}");
        }

        /// <summary>
        /// Generates the acquiring of method value
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>Code to acquire the source value</returns>
        private string GenerateResultSourceFromMethodAcquirement(MethodInfo method)
        {
            var @await = this.Metadata.IsAsync ? "await" : string.Empty;
            List<string> codeCommands = new List<string>();

            var parameterIndex = 0;
            foreach (var parameter in method.GetParameters())
            {
                var parameterDescription =
                    parameter.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;
                var parameterName = parameterDescription?.Name ?? ToCamelCase(parameter.Name);

                string command;
                if (parameter.ParameterType == typeof(RequestContext))
                {
                    command = $"var arg{parameterIndex} = context;";
                }
                else if (parameter.ParameterType == typeof(ApiRequest))
                {
                    command = $"var arg{parameterIndex} = query;";
                }
                else
                {
                    command = $@"
                    var prop{parameterIndex} = query.Arguments != null ? query.Arguments.Property(""{ parameterName}"") : null;
                    var arg{parameterIndex} = prop{parameterIndex} != null 
                            ? prop{parameterIndex}.ToObject<{ToCSharpRepresentation(parameter.ParameterType, true)}>(argumentsSerializer)
                            : default({ToCSharpRepresentation(parameter.ParameterType, true)});
                    ";
                }

                codeCommands.Add(command);
                parameterIndex++;
            }

            return $@"
                {string.Join("\r\n", codeCommands)}
                var resultSource = {@await} sourceTyped.{method.Name}({string.Join(
                ", ",
                Enumerable.Range(0, method.GetParameters().Length).Select(n => $"arg{n}"))});
            ";
        }

        /// <summary>
        /// Generates the acquiring of property value
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>Code to acquire the source value</returns>
        private string GenerateResultSourceFromPropertyAcquirement(PropertyInfo property)
        {
            var @await = this.Metadata.IsAsync ? "await" : string.Empty;
            return $"var resultSource = {@await} sourceTyped.{property.Name};";
        }

        /// <summary>
        /// Generates resolve of the forwarded request
        /// </summary>
        /// <returns>Code to return result</returns>
        private string GenerateForwardedReturn()
        {
            return "return resultSource;";
        }

        /// <summary>
        /// Generates resolve of the request value recursively
        /// </summary>
        /// <returns>Code to return result</returns>
        private string GenerateRecursiveResolve()
        {
            var isSync = !this.Metadata.IsAsync && this.Metadata.ScalarType != EnScalarType.None;

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Scalar)
            {
                return isSync ? "return Task.FromResult<JToken>(new JValue(resultSource));" : "return new JValue(resultSource);";
            }

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Object)
            {
                return $@"
                    {this.GenerateObjectResolve("resultSource", this.Metadata.Type)}
                    return result;
                ";
            }

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Array)
            {
                return $@"
                    var resultArray = new JArray();
                    foreach (var item in resultSource)
                    {{
                        {(this.Metadata.ScalarType == EnScalarType.None ? this.GenerateObjectResolve("item", this.Metadata.Type) : "var result = new JValue(item);")}
                        resultArray.Add(result);
                    }}
                    return {(isSync? "Task.FromResult<JToken>(resultArray)" : "resultArray")};
                ";
            }

            return "return null;";
        }

        /// <summary>
        /// Generates resolve of the object value
        /// </summary>
        /// <param name="itemName">The item variable name</param>
        /// <param name="type">The item type</param>
        /// <returns>Code to return result</returns>
        private string GenerateObjectResolve(string itemName, Type type)
        {
            var prefix = @"
                var result = new JObject();
                ApiRequest fieldQuery;
                PropertyResolver resolver;                
            ";

            List<string> codeCommands = new List<string> { prefix };

            ApiType apiType;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(type.FullName, out apiType))
            {
                throw new InvalidOperationException($"Type {type.FullName} was not described as API type");
            }

            foreach (var apiField in apiType.Fields)
            {
                var command = $@"
                    fieldQuery = query.Fields.FirstOrDefault(f => f.FieldName == ""{apiField.Name}"");
                    if (fieldQuery != null)
                    {{
                        resolver = new {this.Data.ResolverNames[apiType.TypeName][apiField.Name]}();
                        result.Add(""{apiField.Name}"", await resolver.Resolve({itemName}, fieldQuery, context, argumentsSerializer));
                    }}
                ";

                codeCommands.Add(command);
            }

            return string.Join("\r\n", codeCommands);
        }
    }
}