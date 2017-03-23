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

    using ClusterKit.API.Client;
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
        /// Gets or sets the field that is visible only in "input" objects
        /// </summary>
        [DeclareField(Access = EnAccessFlag.Writable)]
        [UsedImplicitly]
        public string ArgumentField { get; set; }

        /// <summary>
        /// Gets or sets the field that is not visible "input" objects, but can be queried
        /// </summary>
        [DeclareField(Access = EnAccessFlag.Queryable)]
        [UsedImplicitly]
        public string ReadOnlyField { get; set; }

        /// <summary>
        /// Gets this for infinite recursive requests
        /// </summary>
        [DeclareField]
        public NestedProvider This => this;

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

        /// <summary>
        /// Test nested mutation
        /// </summary>
        /// <param name="name">The new name of current test object</param>
        /// <returns>Test object after mutation</returns>
        [DeclareMutation]
        [UsedImplicitly]
        public MutationResult<bool> Check(string name)
        {
            return new MutationResult<bool> { Result = true };
        }
    }
}