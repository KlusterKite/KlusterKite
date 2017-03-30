// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishToApiAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Marks properties and methods as publishable to GraphQL api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Marks properties and methods as publishable to GraphQL api
    /// </summary>
    /// <remarks>
    /// Method arguments of types RequestContext and <see cref="ApiRequest"/> will be provided with values automatically from request context
    /// </remarks>
    public abstract class PublishToApiAttribute : ApiDescriptionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishToApiAttribute"/> class.
        /// </summary>
        protected PublishToApiAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishToApiAttribute"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        protected PublishToApiAttribute(string description)
            : base(description)
        {
        }

        /// <summary>
        /// Gets or sets the return type to be described for an api 
        /// </summary>
        /// <remarks>
        /// In case the return type is defined it is assumed that this property / method is forwarding (some other api provider will resolve it's value).
        /// Depending expected return type this should return:
        ///  For scalar values this should be <see cref="Newtonsoft.Json.Linq.JValue"/> or <see cref="Task{TResult}"/> with <see cref="Newtonsoft.Json.Linq.JValue"/> as TResult
        ///  For complex objects values this should be <see cref="Newtonsoft.Json.Linq.JObject"/> or <see cref="Task{TResult}"/> with <see cref="Newtonsoft.Json.Linq.JObject"/> as TResult 
        ///  For array  values this should be <see cref="Newtonsoft.Json.Linq.JArray"/> or <see cref="Task{TResult}"/> with <see cref="Newtonsoft.Json.Linq.JArray"/> as TResult 
        ///  For connection  values this should be <see cref="Newtonsoft.Json.Linq.JObject"/> or <see cref="Task{TResult}"/> with <see cref="Newtonsoft.Json.Linq.JObject"/> as TResult.
        ///  It is expected for JObjects for connections, that they contains exactly two properties - "count" as <see cref="Newtonsoft.Json.Linq.JValue"/> and "items" as <see cref="Newtonsoft.Json.Linq.JArray"/>
        /// </remarks>
        public Type ReturnType { get; set; }

        /// <summary>
        /// Gets the declared member name
        /// </summary>
        /// <param name="memberInfo">The member</param>
        /// <returns>The member name</returns>
        public static string GetMemberName(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute<PublishToApiAttribute>()?.Name ?? ToCamelCase(memberInfo.Name);
        }
    }
}
