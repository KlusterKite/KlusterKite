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
    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Overall debug data of current cluster node template use
    /// </summary>
    [ApiDescription(Description = "Overall debug data of current cluster node template use", Name = "TemplatesStatistics")]
    public class TemplatesUsageStatistics
    {
        /// <summary>
        /// Gets or sets node templates statistics data
        /// </summary>
        [DeclareField("node templates statistics data")]
        public List<Data> Templates { get; set; }

        /// <summary>
        /// Statistics data for template
        /// </summary>
        [ApiDescription("Statistics data for template", Name = "TemplatesStatisticsData")]
        public class Data
        {
            /// <summary>
            /// Gets or sets current active node count
            /// </summary>
            [DeclareField("current active node count")]
            public int ActiveNodes { get; set; }

            /// <summary>
            /// Gets or sets currently configured maximum required active nodes count
            /// </summary>
            [UsedImplicitly]
            [DeclareField("currently configured maximum required active nodes count")]
            public int? MaximumRequiredNodes { get; set; }

            /// <summary>
            /// Gets or sets currently configured minimum required active nodes count
            /// </summary>
            [UsedImplicitly]
            [DeclareField("currently configured minimum required active nodes count")]
            public int MinimumRequiredNodes { get; set; }

            /// <summary>
            /// Gets or sets name of the node template
            /// </summary>
            [DeclareField("name of the node template", IsKey = true)]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the number of currently active nodes, that need to be upgraded
            /// </summary>
            [DeclareField("the number of currently active nodes, that need to be upgraded")]
            public int ObsoleteNodes { get; set; }

            /// <summary>
            /// Gets or sets the number of nodes that are in upgrade process now
            /// </summary>
            [UsedImplicitly]
            [DeclareField("the number of nodes that are in upgrade process now")]
            public int StartingNodes { get; set; }

            /// <summary>
            /// Gets or sets the number of nodes that are in upgrade process now
            /// </summary>
            [DeclareField("the number of nodes that are in upgrade process now")]
            public int UpgradingNodes { get; set; }
        }
    }
}