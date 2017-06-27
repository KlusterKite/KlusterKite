// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFactoryByLogin.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with <see cref="User" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.Core.Monads;
    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Client.ORM;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Data factory to work with <see cref="User"/>
    /// </summary>
    public class UserFactoryByLogin : EntityDataFactorySync<ConfigurationContext, User, string>
    {
        /// <inheritdoc />
        public UserFactoryByLogin(ConfigurationContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override async Task<Maybe<User>> Get(string id)
        {
            return await this.Context.Users
                .Include(nameof(User.Roles))
                .Include($"{nameof(User.Roles)}.{nameof(RoleUser.Role)}")
                .FirstOrDefaultAsync(u => u.Login == id);
        }

        /// <inheritdoc />
        public override string GetId(User obj) => obj.Login;

        /// <inheritdoc />
        public override Expression<Func<User, bool>> GetIdValidationExpression(string id) => obj => id == obj.Login;

        /// <inheritdoc />
        protected override DbSet<User> GetDbSet() => this.Context.Users;
    }
}