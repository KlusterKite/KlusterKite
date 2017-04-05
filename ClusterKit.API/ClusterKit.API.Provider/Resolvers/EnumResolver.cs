// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the EnumResolver type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading.Tasks;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Client;
    using ClusterKit.Security.Attributes;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves a enum value
    /// </summary>
    /// <typeparam name="T">The type of enum</typeparam>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType",
        Justification = "Making use of static properties in generic classes")]
    public class EnumResolver<T> : IResolver
    {
        /// <summary>
        /// A value indicating whether current enum is flags enum
        /// </summary>
        private static bool hasFlags;

        /// <summary>
        /// Initializes static members of the <see cref="EnumResolver{T}"/> class.
        /// </summary>
        static EnumResolver()
        {
            var type = typeof(T);
            if (!type.IsSubclassOf(typeof(Enum)))
            {
                throw new Exception($"{type.Name} should be enum");
            }

            GeneratedType = new ApiEnumType(ApiDescriptionAttribute.GetTypeName(type), Enum.GetNames(type));
            hasFlags = type.GetCustomAttribute<FlagsAttribute>() != null;
        }

        /// <summary>
        /// Gets the generated api type for typed argument
        /// </summary>
        public static ApiType GeneratedType { get; }

        /// <inheritdoc />
        public Task<JToken> ResolveQuery(object source, ApiRequest request, ApiField apiField, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback)
        {
            var value = hasFlags ? new JValue((long)source) : new JValue(source.ToString());
            return Task.FromResult<JToken>(value);
        }

        /// <inheritdoc />
        public ApiType GetElementType()
        {
            return GeneratedType;
        }

        /// <inheritdoc />
        public IEnumerable<ApiField> GetTypeArguments()
        {
            yield break;
        }
    }
}
