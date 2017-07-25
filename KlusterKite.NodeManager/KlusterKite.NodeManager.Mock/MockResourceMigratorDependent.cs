// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockResourceMigratorDependent.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The mock resource to implement <see cref="EnResourceDependencyType.ResourceDependsOnCode" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Mock
{
    using Akka.Configuration;

    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The mock resource to implement <see cref="EnResourceDependencyType.ResourceDependsOnCode"/>
    /// </summary>
    public class MockResourceMigratorDependent : MockResourceMigrator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockResourceMigratorDependent"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        public MockResourceMigratorDependent(Config config) : base(config)
        {
        }
        
        /// <inheritdoc />
        public override string Name => "The mock resources dependent on code";

        /// <inheritdoc />
        public override EnResourceDependencyType DependencyType => EnResourceDependencyType.ResourceDependsOnCode;

        /// <inheritdoc />
        public override string ResourcePointsConfigPath => "KlusterKite.NodeManager.Mock.Dependent.ResourcePoints";

        /// <inheritdoc />
        public override string ResourcesConfigPath => "KlusterKite.NodeManager.Mock.Dependent.Resources";

        /// <inheritdoc />
        protected override string GetResourceName(string resourceCode)
        {
            return $"Dependence resource: {resourceCode}";
        }

        /// <inheritdoc />
        protected override string GetRedisKey(ResourceId resourceId)
        {
            return $"KlusterKite:NodeManager:Mock:Dependent:{resourceId.ConnectionString}";
        }
    }
}
