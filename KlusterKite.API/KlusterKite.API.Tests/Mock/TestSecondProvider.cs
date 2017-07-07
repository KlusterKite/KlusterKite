// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestSecondProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The second provider to test multiple provider configurations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    using ClusterKit.API.Attributes;
    using ClusterKit.API.Provider;

    using JetBrains.Annotations;

    /// <summary>
    /// The second provider to test multiple provider configurations
    /// </summary>
    [ApiDescription(Name = "TestSecondApi")]
    public class TestSecondProvider : ApiProvider
    {
        /// <summary>
        /// Gets the test field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public string SecondApiName => "SecondApiName";
    }
}
