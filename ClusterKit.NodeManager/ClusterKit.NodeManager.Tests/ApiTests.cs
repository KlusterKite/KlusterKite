// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing <see cref="ApiProvider" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using Akka.Actor;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <see cref="ApiProvider"/>
    /// </summary>
    public class ApiTests
    {
        /// <summary>
        /// The test output stream
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ApiTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Testing the api generation
        /// </summary>
        [Fact]
        public void ApiGenerationTest()
        {
            var system = ActorSystem.Create("test");
            var api = new ApiProvider(system);

            foreach (var error in api.GenerationErrors)
            {
                this.output.WriteLine($"Error: {error}");
            }

            foreach (var warning in api.GenerationWarnings)
            {
                this.output.WriteLine($"Warning: {warning}");
            }

            Assert.Equal(0, api.GenerationErrors.Count);
            Assert.Equal(0, api.GenerationWarnings.Count);
        }
    }
}
