// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryContextFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the InMemoryContextFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.EF.InMemory
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Creates the new in memory context for tests
    /// </summary>
    public class InMemoryContextFactory : BaseContextFactory
    {
        /// <inheritdoc />
        public override string ProviderName => "InMemory";

        /// <inheritdoc />
        protected override DbContextOptions<TContext> GetContextOptions<TContext>(
            string connectionString,
            string databaseName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName);
            return optionsBuilder.Options;
        }
    }
}
