// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputContractResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Json contract resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System.Collections.Generic;
    using System.Reflection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Json contract resolver
    /// </summary>
    internal class InputContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// The list of changed names
        /// </summary>
        private readonly Dictionary<MemberInfo, string> names;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputContractResolver"/> class.
        /// </summary>
        /// <param name="names">
        /// The names.
        /// </param>
        public InputContractResolver(Dictionary<MemberInfo, string> names)
        {
            this.names = names;
        }

        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            string name;
            if (this.names.TryGetValue(member, out name))
            {
                property.PropertyName = name;
            }

            return property;
        }
    }
}