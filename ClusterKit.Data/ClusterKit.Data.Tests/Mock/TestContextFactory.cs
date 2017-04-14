// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The test context factory
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.Data.EF;

    using JetBrains.Annotations;

    /// <summary>
    /// The test context factory
    /// </summary>
    public class TestContextFactory : BaseContextFactory<TestDataContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestContextFactory"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public TestContextFactory([NotNull] BaseConnectionManager connectionManager)
            : base(connectionManager)
        {
        }

        /// <summary>
        /// Creates the context and mocks it with data
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>The new context</returns>
        public async Task<TestDataContext> CreateAndUpgradeContext(
            string connectionString,
            string databaseName)
        {
            var context = await this.CreateContext(connectionString, databaseName);
            context.Database.Delete();

            if (!context.Users.Any())
             {
                var user1 = new User
                                {
                                    Login = "user1",
                                    Password = "123",
                                    Uid = Guid.Parse("{72C23018-0C49-4419-8982-D7C0168E8DC2}")
                                };
                 
                var user2 = new User
                                {
                                    Login = "user2",
                                    Password = "123",
                                    Uid = Guid.Parse("{D906C0AB-1108-4B39-B179-C30C83425482}")
                                };

                context.Users.Add(user1);
                context.Users.Add(user2);
                context.SaveChanges();
            }

            return context;
        }
        
        /// <inheritdoc />
        public override async Task<TestDataContext> CreateContext(string connectionString, string databaseName)
        {
            var connection = this.ConnectionManager.CreateConnection(connectionString);
            await connection.OpenAsync();
            var context = Creator(connection, true);
            return context;
        }
    }
}
