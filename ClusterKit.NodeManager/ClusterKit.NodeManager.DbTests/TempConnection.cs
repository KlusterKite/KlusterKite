// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TempConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Opens connection to the newly created database. Database is destroyed after connection is closed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests
{
    using System;
    using System.Configuration;
    using System.Data;

    using ClusterKit.Data.EF.Npgsql;

    using Npgsql;

    using Xunit.Abstractions;

    /// <summary>
    /// Opens connection to the newly created database. Database is destroyed after connection is closed.
    /// </summary>
    public class TempConnection : IDisposable
    {
        /// <summary>
        /// The test output stream
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// The database name
        /// </summary>
        private string databaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempConnection"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        private TempConnection(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Gets the database connection
        /// </summary>
        public NpgsqlConnection Connection { get; private set; }

        /// <summary>
        /// Gets the actual connection string
        /// </summary>
        public string ConnectionString =>
            $"{ConfigurationManager.ConnectionStrings["testPostgres"].ConnectionString};Database={this.databaseName}";

        /// <summary>
        /// The database connection creation
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <returns>The connection</returns>
        public static TempConnection Create(ITestOutputHelper output)
        {
            var tempConnection = new TempConnection(output)
                                     {
                                         Connection =
                                             new NpgsqlConnection(
                                                 ConfigurationManager
                                                     .ConnectionStrings["testPostgres"]
                                                     .ConnectionString),
                                         databaseName = $"test_{Guid.NewGuid():N}"
                                     };

            try
            {
                tempConnection.Connection.Open();
                output.WriteLine($"Creating temp database {tempConnection.databaseName}");
                new ConnectionManager().CheckCreateDatabase(tempConnection.Connection, tempConnection.databaseName);
                tempConnection.Connection.ChangeDatabase(tempConnection.databaseName);
                return tempConnection;
            }
            catch (Exception)
            {
                tempConnection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts wrapper to the original object
        /// </summary>
        /// <param name="obj">Wrapped object</param>
        public static implicit operator NpgsqlConnection(TempConnection obj)
        {
            return obj.Connection;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.Connection.Dispose();

            using (var dropDatabaseConnection =
                new NpgsqlConnection(ConfigurationManager.ConnectionStrings["testPostgres"].ConnectionString))
            {
                dropDatabaseConnection.Open();
                this.output.WriteLine($"removing temp database {this.databaseName}");
                using (var cmd = dropDatabaseConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $@"SELECT pg_terminate_backend(pg_stat_activity.pid)
                                FROM pg_stat_activity
                                WHERE pg_stat_activity.datname = '{this.databaseName}'
                                  AND pid <> pg_backend_pid()";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = dropDatabaseConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"DROP DATABASE \"{this.databaseName}\"";
                    cmd.ExecuteNonQuery();
                }

                this.output.WriteLine($"temp database {this.databaseName} was removed");
            }
        }
    }
}