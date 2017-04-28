// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestConnectionManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The manager that creates connections to in-memory database
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System.Data.Common;

    using ClusterKit.Data.EF;

    /// <summary>
    /// The manager that creates connections to in-memory database
    /// </summary>
    public class TestConnectionManager : BaseConnectionManager
    {
        /// <summary>
        /// The instance name
        /// </summary>
        private readonly DatabaseInstanceName instanceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestConnectionManager"/> class.
        /// </summary>
        /// <param name="instanceName">
        /// The instance name.
        /// </param>
        public TestConnectionManager(DatabaseInstanceName instanceName)
        {
            this.instanceName = instanceName;
        }

        /// <inheritdoc />
        public override string ProviderInvariantName => "Test";

        /// <inheritdoc />
        public override bool CheckDatabaseExistence(DbConnection connection, string databaseName)
        {
            return true;
        }

        /// <inheritdoc />
        public override void CheckCreateDatabase(DbConnection connection, string databaseName)
        {
        }

        /// <inheritdoc />
        public override DbConnection CreateConnection(string connectionString)
        {
            return Effort.DbConnectionFactory.CreatePersistent(this.instanceName.Name);
        }

        /// <inheritdoc />
        public override void SwitchDatabase(DbConnection connection, string databaseName)
        {
        }
    }
}
