// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockResourceMigratorDependence.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The mock resource to implement <see cref="EnResourceDependencyType.CodeDependsOnResource" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Mock
{
    using Akka.Configuration;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The mock resource to implement <see cref="EnResourceDependencyType.CodeDependsOnResource"/>
    /// </summary>
    public class MockResourceMigratorDependence : MockResourceMigrator
    {
        /// <inheritdoc />
        public MockResourceMigratorDependence(Config config)
            : base(config)
        {
        }

        /// <inheritdoc />
        public override string Name => "The mock resources that code depends on";

        /// <inheritdoc />
        public override EnResourceDependencyType DependencyType => EnResourceDependencyType.CodeDependsOnResource;

        /// <inheritdoc />
        public override string ResourcePointsConfigPath => "KlusterKite.NodeManager.Mock.Dependence.ResourcePoints";

        /// <inheritdoc />
        public override string ResourcesConfigPath => "KlusterKite.NodeManager.Mock.Dependence.Resources";

        /// <inheritdoc />
        protected override string GetResourceName(string resourceCode)
        {
            return $"Dependence resource: {resourceCode}";
        }

        /// <inheritdoc />
        protected override string GetRedisKey(ResourceId resourceId)
        {
            return $"KlusterKite:NodeManager:Mock:Dependence:{resourceId.ConnectionString}";
        }
    }
}
