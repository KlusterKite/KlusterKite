// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFactoryByUid.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with <see cref="User" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Data.Entity;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.Core.Monads;
    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// Data factory to work with <see cref="User"/>
    /// </summary>
    public class UserFactoryByUid : EntityDataFactorySync<ConfigurationContext, User, Guid>
    {
        /// <inheritdoc />
        public UserFactoryByUid(ConfigurationContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override async Task<Maybe<User>> Get(Guid id)
        {
            return await this.Context.Users.Include(nameof(User.Roles)).FirstOrDefaultAsync(u => u.Uid == id);
        }

        /// <inheritdoc />
        public override Guid GetId(User obj) => obj.Uid;

        /// <inheritdoc />
        public override Expression<Func<User, bool>> GetIdValidationExpression(Guid id) => obj => id == obj.Uid;

        /// <inheritdoc />
        protected override DbSet<User> GetDbSet() => this.Context.Users;
    }
}