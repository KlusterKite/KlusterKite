// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeclareFieldAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Declare method as published to api and a data mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes
{
    using System;

    /// <summary>
    /// Declare method as published to api
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DeclareFieldAttribute : PublishToApiAttribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether current property is entity identification key.
        /// </summary>
        public bool IsKey { get; set; }
    }
}