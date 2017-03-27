// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestRolesFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The roles data factory
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System;
    using System.Data.Entity;
    using System.Linq.Expressions;

    using ClusterKit.Data.EF;

    /// <summary>
    /// The roles data factory
    /// </summary>
    public class TestRolesFactory : EntityDataFactorySync<TestDataContext, Role, Guid>
    {
        /// <inheritdoc />
        public TestRolesFactory(TestDataContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override Guid GetId(Role obj) => obj.Uid;

        /// <inheritdoc />
        public override Expression<Func<Role, bool>> GetIdValidationExpression(Guid id) => obj => obj.Uid == id;

        /// <inheritdoc />
        protected override DbSet<Role> GetDbSet()
        {
            return this.Context.Roles;
        }
    }
}