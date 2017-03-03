// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Nested API
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    using System.Threading.Tasks;

    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Nested API
    /// </summary>
    [UsedImplicitly]
    [ApiDescription(Description = "Nested API", Name = "NestedProvider")]
    public class NestedProvider
    {
        /// <summary>
        /// Async scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public Task<string> AsyncScalarField => Task.FromResult("AsyncScalarField");

        /// <summary>
        /// Gets or sets the sync scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public string SyncScalarField { get; set; } = "SyncScalarField";

        /// <summary>
        /// Test nested mutation
        /// </summary>
        /// <param name="name">The new name of current test object</param>
        /// <returns>Test object after mutation</returns>
        [DeclareMutation]
        [UsedImplicitly]
        public TestObject SetName(string name)
        {
            return new TestObject { Name = name };
        }
    }
}