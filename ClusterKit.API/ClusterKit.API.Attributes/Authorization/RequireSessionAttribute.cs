// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireSessionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the RequireSessionAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Attributes.Authorization
{
    using System;

    /// <summary>
    /// Marks property / method published for API that access to it requires valid authentication session
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class RequireSessionAttribute : Attribute
    {
    }
}
