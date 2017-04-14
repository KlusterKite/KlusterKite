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
    using ClusterKit.Data.Tests.Mock;
    using Xunit;

    /// <summary>
    /// Tests <see cref="BaseContextFactory{TContext}"/>
    /// </summary>
    public class ContextFactoryTest
    {
        /// <summary>
        /// Tests that <see cref="BaseContextFactory{TContext}"/> can create contexts
        /// </summary>
        [Fact]
        public void CreatorTest()
        {
            var creator = BaseContextFactory<TestDataContext>.Creator;
            Assert.NotNull(creator);

            var context = creator(null, true);
            Assert.NotNull(context);
        }
    }
}