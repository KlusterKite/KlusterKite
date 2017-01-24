// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestConnectionManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Connection manager for testing. Creates <seealso cref="MockConnection" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.TestKit
{
    using System.Data.Common;

    /// <summary>
    /// Connection manager for testing. Creates <seealso cref="MockConnection"/>
    /// </summary>
    public class TestConnectionManager : BaseConnectionManager
    {
        /// <summary>
        /// Checks for database existence. In case it is not - creates it
        /// </summary>
        /// <param name="connection">Opened database connection</param>
        /// <param name="databaseName">Database name to check</param>
        public override void CheckCreateDatabase(DbConnection connection, string databaseName)
        {
        }

        /// <summary>
        /// Creates connection to database
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Database connection</returns>
        /// <remarks>Don't forget to dispose it</remarks>
        public override DbConnection CreateConnection(string connectionString)
        {
            return new MockConnection();
        }
    }
}