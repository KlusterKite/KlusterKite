// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TemplatesUsageStatistics.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the TemplateStatistics type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using System.Collections.Generic;

    /// <summary>
    /// Overall debug data of current cluster node template use
    /// </summary>
    public class TemplatesUsageStatistics
    {
        /// <summary>
        /// Gets or sets node templates statistics data
        /// </summary>
        public List<TemplateUsageStatistics> Templates { get; set; }

        /// <summary>
        /// Statistics data for template
        /// </summary>
        public class TemplateUsageStatistics
        {
            /// <summary>
            /// Gets or sets current active node count
            /// </summary>
            public int ActiveNodes { get; set; }

            /// <summary>
            /// Gets or sets currently configured maximum required active nodes count
            /// </summary>
            public int? MaximumRequiredNodes { get; set; }

            /// <summary>
            /// Gets or sets currently configured minimum required active nodes count
            /// </summary>
            public int MinimumRequiredNodes { get; set; }

            /// <summary>
            /// Gets or sets name of the node template
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the number of currently active nodes, that need to be upgraded
            /// </summary>
            public int ObsoleteNodes { get; set; }

            /// <summary>
            /// Gets or sets the number of nodes that are in upgrade process now
            /// </summary>
            public int UpgradingNodes { get; set; }
        }
    }
}