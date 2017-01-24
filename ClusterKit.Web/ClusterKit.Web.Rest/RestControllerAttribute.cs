// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestControllerAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Provides route information for
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.CRUDS
{
    using System;

    using ClusterKit.Web.Rest;

    /// <summary>
    /// Provides route information for <seealso cref="BaseRestController{TObject,TId}"/>
    /// </summary>
    public class RestControllerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestControllerAttribute"/> class.
        /// </summary>
        /// <param name="path">
        /// Route path for crud operations
        /// </param>
        public RestControllerAttribute(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Gets route path for crud operations
        /// </summary>
        public string Path { get; set; }
    }
}