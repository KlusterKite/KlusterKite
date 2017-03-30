// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeclareMutationAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the DeclareMutationAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes
{
    using System;

    /// <summary>
    /// Declare method as published to api and a data mutation
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DeclareMutationAttribute : PublishToApiAttribute
    {
    }
}
