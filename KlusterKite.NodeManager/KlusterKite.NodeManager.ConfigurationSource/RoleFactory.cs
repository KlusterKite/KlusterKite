// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with <see cref="Role" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using KlusterKite.Core.Monads;
    using KlusterKite.Data.EF;
    using KlusterKite.NodeManager.Client.ORM;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Data factory to work with <see cref="Role"/>
    /// </summary>
    public class RoleFactory : EntityDataFactorySync<ConfigurationContext, Role, Guid>
    {
        /// <inheritdoc />
        public RoleFactory(ConfigurationContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override async Task<Maybe<Role>> Get(Guid id)
        {
            return await this.Context.Roles.Include(nameof(Role.Users)).FirstOrDefaultAsync(r => r.Uid == id);
        }

        /// <inheritdoc />
        public override Guid GetId(Role obj) => obj.Uid;

        /// <inheritdoc />
        public override Expression<Func<Role, bool>> GetIdValidationExpression(Guid id) => obj => id == obj.Uid;

        /// <inheritdoc />
        protected override DbSet<Role> GetDbSet() => this.Context.Roles;
    }
}