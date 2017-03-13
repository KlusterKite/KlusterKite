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
    using System.Data.Entity;

    using ClusterKit.Data.EF;
    using ClusterKit.Data.Tests.Mock;

    using Moq;
    using Xunit;

    /// <summary>
    /// Tests <see cref="BaseContextFactory{TContext,TMigrationConfiguration}"/>
    /// </summary>
    public class ContextFactoryTest
    {
        /// <summary>
        /// Tests that <see cref="BaseContextFactory{TContext,TMigrationConfiguration}"/> can create contexts
        /// </summary>
        [Fact]
        public void CreatorTest()
        {
            var creator = BaseContextFactory<TestDataContext, TestDataContextMigrationConfiguration>.Creator;
            Assert.NotNull(creator);

            var context = creator(null, true);
            Assert.NotNull(context);
        }

        /// <summary>
        /// Tests mocking work
        /// </summary>
        [Fact]
        public void MockTest()
        {
            var usersMock = new Mock<DbSet<User>>();
            var rolesMock = new Mock<DbSet<Role>>();
            var context = new Mock<TestDataContext>();
            context.Setup(m => m.Roles).Returns(rolesMock.Object);
            context.Setup(m => m.Users).Returns(usersMock.Object);
        }
    }
}