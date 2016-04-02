// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvailableTemplatesRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Debug request - checks containers for the node
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    /// <summary>
    /// Debug request - checks containers for the node
    /// </summary>
    public class AvailableTemplatesRequest
    {
        /// <summary>
        /// Gets or sets the container type
        /// </summary>
        public string ContainerType { get; set; }
    }
}