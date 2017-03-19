// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper, that generates C# code for the implementation of <see cref="ObjectResolver" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.API.Provider.Resolvers;
    using ClusterKit.Security.Client;

    /// <summary>
    /// Helper, that generates C# code for the implementation of <see cref="ObjectResolver"/>
    /// </summary>
    internal class ObjectResolverGenerator : ResolverGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectResolverGenerator"/> class.
        /// </summary>
        /// <param name="objectType">
        /// The object type.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public ObjectResolverGenerator(Type objectType, AssembleTempData data)
            : base(objectType, data)
        {
        }

        /// <inheritdoc />
        protected override string ClassName => "ObjectResolver"
                                               + $"_{Regex.Replace(NamingUtilities.ToCSharpRepresentation(this.ObjectType), "[^a-zA-Z0-9]", string.Empty)}"
                                               + $"_{this.Uid:N}";

        /// <summary>
        /// Creates c# code for defined parameters
        /// </summary>
        /// <returns>The field resolver definition in C#</returns>
        public override string Generate()
        {
            return $@"
                    namespace ClusterKit.API.Provider.Dynamic 
                    {{
                        using System;
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        using System.Linq;
                        using System.Linq.Expressions;

                        using Newtonsoft.Json;
                        using Newtonsoft.Json.Linq;
                    
                        using ClusterKit.Security.Client;
                        using ClusterKit.API.Client;
                        using ClusterKit.API.Provider.Resolvers;

                        // Object resolver for {NamingUtilities.ToCSharpRepresentation(this.ObjectType)}
                        public class {this.ClassName} : ObjectResolver
                        {{
                                private static Dictionary<string, Func<{NamingUtilities.ToCSharpRepresentation(this.ObjectType)},ApiRequest,RequestContext,JsonSerializer,Task<ResolvePropertyResult>>>
                                    PropertyResolvers = new Dictionary<string, Func<{NamingUtilities.ToCSharpRepresentation(
                this.ObjectType)},ApiRequest,RequestContext,JsonSerializer,Task<ResolvePropertyResult>>>
                                    {{
                                    {string.Join(",", this.GeneratePropertyResolvers())}
                                    }};

                                public override async Task<ResolvePropertyResult> ResolvePropertyValue(
                                    object source,
                                    ApiRequest request,
                                    RequestContext context,
                                    JsonSerializer argumentsSerializer,
                                    Action<Exception> onErrorCallback)
                                {{
                                        var typedSource = ({NamingUtilities.ToCSharpRepresentation(this.ObjectType)})source;
                                        if (typedSource == null)
                                        {{
                                            if (onErrorCallback != null) onErrorCallback(new Exception(""source is null or of wrong type""));
                                            return null;
                                        }}

                                        Func<{NamingUtilities.ToCSharpRepresentation(this.ObjectType)},ApiRequest,RequestContext,JsonSerializer,Task<ResolvePropertyResult>> resolver;
                                        if (!PropertyResolvers.TryGetValue(request.FieldName, out resolver))
                                        {{
                                            if (onErrorCallback != null) onErrorCallback(new Exception(""There is no resolver for field "" + request.FieldName));
                                            return null;
                                        }}

                                        return await resolver(typedSource, request, context, argumentsSerializer);
                                }}
                        }}
                   }}
            ";
        }

        /// <summary>
        /// Generates property resolvers for the resolver
        /// </summary>
        /// <returns>The list of property resolvers</returns>
        protected IEnumerable<string> GeneratePropertyResolvers()
        {
            ApiType apiType;
            Dictionary<string, MemberInfo> members;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(this.ObjectType.FullName, out apiType)
                || !this.Data.Members.TryGetValue(apiType.TypeName, out members))
            {
                throw new Exception($"{NamingUtilities.ToCSharpRepresentation(this.ObjectType)} has no ApiType");
            }

            var apiObjectType = apiType as ApiObjectType;
            if (apiObjectType == null)
            {
                throw new Exception($"{NamingUtilities.ToCSharpRepresentation(this.ObjectType)} is not an object type");
            }

            foreach (var field in apiObjectType.Fields.Union(apiObjectType.DirectMutations ?? new List<ApiField>()))
            {
                MemberInfo member;
                if (!members.TryGetValue(field.Name, out member))
                {
                    continue;
                }

                var methodInfo = member as MethodInfo;
                var propertyInfo = member as PropertyInfo;

                var metaData = ApiProvider.GenerateTypeMetadata(
                    propertyInfo?.PropertyType ?? methodInfo?.ReturnType,
                    member.GetCustomAttribute<PublishToApiAttribute>());

                var asyncPrefix = metaData.IsAsync ? "async" : string.Empty;
                var returnResult = metaData.IsAsync
                                       ? "new ResolvePropertyResult { Value = fieldValue, Resolver = resolver }"
                                       : "Task.FromResult(new ResolvePropertyResult { Value = fieldValue, Resolver = resolver })";

                var valueGetter = methodInfo != null
                                      ? this.GenerateResultSourceFromMethodAcquirement(metaData, methodInfo)
                                      : this.GenerateResultSourceFromPropertyAcquirement(metaData, propertyInfo);

                var resolverGetter = this.GenerateResolverFromTypeMetaData(metaData, field);

                // Func<{ToCSharpRepresentation(this.objectType)},JObject,RequestContext,JsonSerializer,Task<ResolvePropertyResult>>
                yield return $@"
                    {{
                        ""{field.Name}"", 
                        {asyncPrefix} (source, request, context, serializer) => 
                        {{
                            {valueGetter} 
                            {resolverGetter}                           
                            return {returnResult};
                        }}
                    }}";
            }
        }

        /// <summary>
        /// Generates resolver acquirement for field type 
        /// </summary>
        /// <param name="metaData">The field type metadata</param>
        /// <param name="field">The field description</param>
        /// <returns>The resolver acquirement</returns>
        protected string GenerateResolverFromTypeMetaData(TypeMetadata metaData, ApiField field)
        {
            if (metaData.IsForwarding)
            {
                return "var resolver = new ForwarderResolver();";
            }

            if (metaData.MetaType == TypeMetadata.EnMetaType.Connection)
            {
                return $"var resolver = new {this.Data.ConnectionResolverNames[metaData.TypeName]}();";
            }

            var endTypeResolver = metaData.ScalarType != EnScalarType.None
                                      ? $"new ScalarResolver<{NamingUtilities.ToCSharpRepresentation(metaData.Type)}>()"
                                      : metaData.Type.IsSubclassOf(typeof(Enum))
                                          ? metaData.Type.GetCustomAttribute<FlagsAttribute>() != null
                                                ? $"new ScalarResolver<{NamingUtilities.ToCSharpRepresentation(Enum.GetUnderlyingType(metaData.Type))}>()"
                                                : "new StringResolver()"
                                          : $"new {this.Data.ObjectResolverNames[field.TypeName]}()";

            switch (metaData.MetaType)
            {
                case TypeMetadata.EnMetaType.Scalar:
                case TypeMetadata.EnMetaType.Object:
                    return $"var resolver = {endTypeResolver};";
                case TypeMetadata.EnMetaType.Array:
                    return $"var resolver = new CollectionResolver({endTypeResolver});";
                case TypeMetadata.EnMetaType.Connection:
                    return $"var resolver = {endTypeResolver};";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Generates the acquiring of method value
        /// </summary>
        /// <param name="metadata">The return type metadata</param>
        /// <param name="method">The method</param>
        /// <returns>Code to acquire the source value</returns>
        protected string GenerateResultSourceFromMethodAcquirement(TypeMetadata metadata, MethodInfo method)
        {
            var await = metadata.IsAsync ? "await" : string.Empty;
            List<string> codeCommands = new List<string>();

            var parameterIndex = 0;
            foreach (var parameter in method.GetParameters())
            {
                var parameterDescription =
                    parameter.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;
                var parameterName = parameterDescription?.Name ?? ApiDescriptionAttribute.ToCamelCase(parameter.Name);

                string command;
                if (parameter.ParameterType == typeof(RequestContext))
                {
                    command = $"var arg{parameterIndex} = context;";
                }
                else if (parameter.ParameterType == typeof(ApiRequest))
                {
                    command = $"var arg{parameterIndex} = request;";
                }
                else
                {
                    command = $@"                    
                    var prop{parameterIndex} = arguments != null ? arguments.Property(""{parameterName}"") : null;
                    var arg{parameterIndex} = prop{parameterIndex} != null && prop{parameterIndex}.Value != null
                            ? prop{parameterIndex}.Value.ToObject<{NamingUtilities.ToCSharpRepresentation(parameter.ParameterType)}>(serializer)
                            : default({NamingUtilities.ToCSharpRepresentation(parameter.ParameterType)});
                    ";
                }

                codeCommands.Add(command);
                parameterIndex++;
            }

            var execution = $@" {@await} source.{method.Name}({string.Join(", ",
                Enumerable.Range(0, method.GetParameters().Length).Select(n => $"arg{n}"))})";
            var acquirement = metadata.ConverterType == null
                ? $"var fieldValue = {execution};"
                : $"var fieldValue = new {NamingUtilities.ToCSharpRepresentation(metadata.ConverterType)}().Convert({execution});";
            return $@"
                JObject arguments = request.Arguments;
                {string.Join("\r\n", codeCommands)}
                {acquirement}
            ";
        }

        /// <summary>
        /// Generates the acquiring of property value
        /// </summary>
        /// <param name="metadata">The return type metadata</param>
        /// <param name="property">The property</param>
        /// <returns>Code to acquire the source value</returns>
        protected string GenerateResultSourceFromPropertyAcquirement(TypeMetadata metadata, PropertyInfo property)
        {
            var await = metadata.IsAsync ? "await" : string.Empty;
            var acquirement = metadata.ConverterType == null
                                  ? $"var fieldValue = {@await} source.{property.Name};"
                                  : $"var fieldValue = new {NamingUtilities.ToCSharpRepresentation(metadata.ConverterType)}().Convert({@await} source.{property.Name});";
            return acquirement;
        }
    }
}