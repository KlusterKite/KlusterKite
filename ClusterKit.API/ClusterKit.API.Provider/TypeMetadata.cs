// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeMetadata.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Property or method return type description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;

    using ClusterKit.API.Client;

    /// <summary>
    /// Property or method return type description
    /// </summary>
    internal class TypeMetadata
    {
        /// <summary>
        /// The return types of a field
        /// </summary>
        public enum EnMetaType
        {
            /// <summary>
            /// This is scalar field
            /// </summary>
            Scalar,

            /// <summary>
            /// The field is an object
            /// </summary>
            Object,

            /// <summary>
            /// The field is an array
            /// </summary>
            Array,

            /// <summary>
            /// The field represents connection
            /// </summary>
            Connection
        }

        /// <summary>
        /// Gets or sets a value indicating whether  this property / method has asynchronous access
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property / method forwards resolve to other API provider
        /// </summary>
        public bool IsForwarding { get; set; }

        /// <summary>
        /// Gets or sets the meta type
        /// </summary>
        public EnMetaType MetaType { get; set; }

        /// <summary>
        /// Gets or sets the parsed scalar type
        /// </summary>
        public EnScalarType ScalarType { get; set; }

        /// <summary>
        /// Gets or sets the true returning type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Checks the type to be scalar
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The corresponding scalar type</returns>
        public static EnScalarType CheckScalarType(Type type)
        {
            var nullable = CheckType(type, typeof(Nullable<>));
            if (nullable != null)
            {
                type = nullable.GenericTypeArguments[0];
            }

            if (type == typeof(string))
            {
                return EnScalarType.String;
            }

            if (type == typeof(bool))
            {
                return EnScalarType.Boolean;
            }

            if (type == typeof(Guid))
            {
                return EnScalarType.Guid;
            }

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(uint)
                || type == typeof(ulong) || type == typeof(ushort))
            {
                return EnScalarType.Integer;
            }

            if (type == typeof(float) || type == typeof(double))
            {
                return EnScalarType.Float;
            }

            if (type == typeof(decimal))
            {
                return EnScalarType.Decimal;
            }

            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                return EnScalarType.DateTime;
            }

            return EnScalarType.None;
        }

        /// <summary>
        /// Checks the interface implementation
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="typeToCheck">The type of an interface</param>
        /// <returns>The interface or null</returns>
        public static Type CheckType(Type type, Type typeToCheck)
        {
            if (type == typeToCheck)
            {
                return type;
            }

            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeToCheck)
            {
                return type;
            }

            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface == typeToCheck)
                {
                    return @interface;
                }

                if (@interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == typeToCheck)
                {
                    return @interface;
                }
            }

            foreach (var baseType in GetBaseTypes(type))
            {
                if (baseType == typeToCheck)
                {
                    return baseType;
                }

                if (baseType.IsConstructedGenericType && baseType.GetGenericTypeDefinition() == typeToCheck)
                {
                    return baseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="ClusterKit.API.Client.EnFieldFlags"/> from metadata
        /// </summary>
        /// <returns>The field flags</returns>
        public EnFieldFlags GetFlags()
        {
            EnFieldFlags flags;
            switch (this.MetaType)
            {
                case EnMetaType.Object:
                    flags = EnFieldFlags.None;
                    break;
                case EnMetaType.Array:
                    flags = EnFieldFlags.IsArray;
                    break;
                case EnMetaType.Connection:
                    flags = EnFieldFlags.IsConnection;
                    break;
                case EnMetaType.Scalar:
                    flags = EnFieldFlags.None;
                    break;
                default:
                    flags = EnFieldFlags.None;
                    break;
            }

            return flags;
        }

        /// <summary>
        /// Gets the base type hierarchy
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The list of base types</returns>
        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type.BaseType == null)
            {
                yield break;
            }

            yield return type.BaseType;
            foreach (var baseType in GetBaseTypes(type.BaseType))
            {
                yield return baseType;
            }
        }
    }
}