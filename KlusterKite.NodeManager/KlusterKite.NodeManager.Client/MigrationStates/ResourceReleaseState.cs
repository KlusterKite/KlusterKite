// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceReleaseState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The resource description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The resource description
    /// </summary>
    [ApiDescription("The resource description", Name = "ResourceReleaseState")]
    public class ResourceReleaseState
    {
        /// <summary>
        /// Gets or sets the human readable resource name
        /// </summary>
        [DeclareField("the human readable resource name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource identification
        /// </summary>
        [DeclareField("the resource identification", IsKey = true)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the current migration point of the resource
        /// </summary>
        [DeclareField("the current migration point of the resource")]
        public string CurrentPoint { get; set; }
    }
}
