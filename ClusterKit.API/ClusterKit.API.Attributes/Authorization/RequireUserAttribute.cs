// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireUserAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Marks property / method published for API that access to its requires valid authentication session with authenticated user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Attributes.Authorization
{
    using System;

    /// <summary>
    /// Marks property / method published for API that access to it requires valid authentication session with authenticated user
    /// </summary>
    /// <remarks>
    /// This rule will be ignored in case of valid authentication session absence.
    /// In case of strict authentication session need please use in combination with <see cref="RequireSessionAttribute"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class RequireUserAttribute : Attribute
    {
    }
}