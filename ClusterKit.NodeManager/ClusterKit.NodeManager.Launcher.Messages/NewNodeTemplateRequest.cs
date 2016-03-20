// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewNodeTemplateRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Request from
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Launcher.Messages
{
    /// <summary>
    /// Request from newly started node
    /// </summary>
    public class NewNodeTemplateRequest
    {
        /// <summary>
        /// Gets or sets container type to start node
        /// </summary>
        public string ContainerType { get; set; }
    }
}