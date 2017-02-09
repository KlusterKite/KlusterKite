// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishToApiAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Marks class properties and methods to be published in GraphQL api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    using System;

    /// <summary>
    /// Marks class properties and methods to be published in GraphQL api
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class PublishToApiAttribute : Attribute
    {
    }
}
