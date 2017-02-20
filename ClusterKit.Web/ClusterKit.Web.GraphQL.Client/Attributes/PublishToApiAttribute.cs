// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishToApiAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Marks properties and methods as publishable to GraphQL api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client.Attributes
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Marks properties and methods as publishable to GraphQL api
    /// </summary>
    /// <remarks>
    /// Method arguments of types RequestContext and <see cref="ApiRequest"/> will be provided with values automatically from request context
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public abstract class PublishToApiAttribute : ApiDescriptionAttribute
    {
        /// <summary>
        /// Gets or sets the return type to be described for an api 
        /// </summary>
        /// <remarks>
        /// In case the return type is defined it is assumed that this property / method is forwarding (some other api provider will resolve it's value).
        /// Depending expected return type this should return:
        ///  For scalar values this should be <see cref="JValue"/> or <see cref="Task{TResult}"/> with <see cref="JValue"/> as TResult
        ///  For complex objects values this should be <see cref="JObject"/> or <see cref="Task{TResult}"/> with <see cref="JObject"/> as TResult 
        ///  For array  values this should be <see cref="JArray"/> or <see cref="Task{TResult}"/> with <see cref="JArray"/> as TResult 
        ///  For connection  values this should be <see cref="JObject"/> or <see cref="Task{TResult}"/> with <see cref="JObject"/> as TResult.
        ///  It is expected for JObjects for connections, that they contains exactly two properties - "count" as <see cref="JValue"/> and "items" as <see cref="JArray"/>
        /// </remarks>
        public Type ReturnType { get; set; }
    }
}
