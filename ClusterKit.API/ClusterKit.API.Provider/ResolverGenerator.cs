// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper, that generates C# code for the resolvers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The base class for resolver generators
    /// </summary>
    internal abstract class ResolverGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolverGenerator"/> class.
        /// </summary>
        /// <param name="objectType">
        /// The object to generate resolver for
        /// </param>
        /// <param name="data">
        /// The global assembled data.
        /// </param>
        protected ResolverGenerator(Type objectType, AssembleTempData data)
        {
            this.ObjectType = objectType;
            this.Data = data;
        }

        /// <summary>
        /// Gets the generated class name
        /// </summary>
        public string FullClassName => $"ClusterKit.API.Provider.Dynamic.{this.ClassName}";

        /// <summary>
        /// Gets the generated class name
        /// </summary>
        protected abstract string ClassName { get; }

        /// <summary>
        /// Gets the global assembled data
        /// </summary>
        protected AssembleTempData Data { get; }

        /// <summary>
        /// Gets the type of object to generate resolver for
        /// </summary>
        protected Type ObjectType { get; }

        /// <summary>
        /// Gets the resolver uid
        /// </summary>
        protected Guid Uid { get; } = Guid.NewGuid();

        /// <summary>
        /// Creates c# code for defined parameters
        /// </summary>
        /// <returns>The field resolver definition in C#</returns>
        public abstract string Generate();

        /// <summary>
        /// Creates valid type name representation
        /// </summary>
        /// <remarks>
        /// Original code: <see href="http://stackoverflow.com/questions/2579734/get-the-type-name"/>
        /// </remarks>
        /// <param name="type">The type</param>
        /// <param name="trimArgCount">A value indicating whether to trim arguments count</param>
        /// <returns>A valid C# name</returns>
        internal static string ToCSharpRepresentation(Type type, bool trimArgCount = true)
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
    }
}