// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextFactoryTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Tests <see cref="BaseContextFactory{TContext,TMigrationConfiguration}" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests
{
    using ClusterKit.Data.EF;
    using ClusterKit.Data.EF.InMemory;
    using ClusterKit.Data.Tests.Mock;
    using Xunit;

    /// <summary>
    /// Tests <see cref="BaseContextFactory"/>
    /// </summary>
    public class ContextFactoryTest
    {
        /// <summary>
        /// Tests that <see cref="BaseContextFactory"/> can create contexts
        /// </summary>
        [Fact]
        public void CreatorTest()
        {
            var contextFactory = new InMemoryContextFactory();
            var context = contextFactory.CreateContext<TestDataContext>((string)null);
            Assert.NotNull(context);
        }
    }
}