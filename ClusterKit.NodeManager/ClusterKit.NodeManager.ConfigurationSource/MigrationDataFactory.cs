// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationDataFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with <see cref="Migration" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Linq.Expressions;

    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Client.ORM;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Data factory to work with <see cref="Migration"/>
    /// </summary>
    public class MigrationDataFactory : EntityDataFactory<ConfigurationContext, Migration, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationDataFactory"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public MigrationDataFactory(ConfigurationContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override int GetId(Migration obj)
        {
            return obj.Id;
        }

        /// <inheritdoc />
        public override Expression<Func<Migration, bool>> GetIdValidationExpression(int id)
        {
            return obj => obj.Id == id;
        }

        /// <inheritdoc />
        protected override DbSet<Migration> GetDbSet()
        {
            return this.Context.Migrations;
        }
    }
}
