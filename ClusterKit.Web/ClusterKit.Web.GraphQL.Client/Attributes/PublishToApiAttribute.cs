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

    /// <summary>
    /// Marks properties and methods as publishable to GraphQL api
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class PublishToApiAttribute : ApiDescriptionAttribute
    {
    }
}
