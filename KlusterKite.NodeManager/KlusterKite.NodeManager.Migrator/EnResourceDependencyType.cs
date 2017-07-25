// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnResourceDependencyType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The type of resource dependency
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Migrator
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The type of resource dependency
    /// </summary>
    [ApiDescription("The type of resource dependency", Name = "EnResourceType")]
    public enum EnResourceDependencyType
    {
        /// <summary>
        /// The code depends on resource (so resource should be updated prior to code)
        /// </summary>
        [ApiDescription("The code depends on resource (so resource should be updated prior to code)")]
        CodeDependsOnResource,

        /// <summary>
        /// The resource depends on code (so code should be updated prior to resource
        /// </summary>
        [ApiDescription("The resource depends on code (so code should be updated prior to resource")]
        ResourceDependsOnCode
    }
}