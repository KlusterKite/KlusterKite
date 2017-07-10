// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireSessionAttribute.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the RequireSessionAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes.Authorization
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
