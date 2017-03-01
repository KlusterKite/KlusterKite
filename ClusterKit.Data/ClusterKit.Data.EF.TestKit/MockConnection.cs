// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Mocks database connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.TestKit
{
    using System;
    using System.Data;
    using System.Data.Common;

    /// <summary>
    /// Mocks database connection
    /// </summary>
    public class MockConnection : DbConnection
    {
        /// <summary>
        /// Specified database name
        /// </summary>
        /// <remarks>It doesn't matter. It's mock.</remarks>
        private string database;

        /// <summary>
        /// Gets or sets the string used to open the connection.
        /// </summary>
        /// <returns>
        /// The connection string used to establish the initial connection. The exact contents of the connection string depend on the specific data source for this connection. The default value is an empty string.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string ConnectionString { get; set; }

        /// <summary>
        /// Gets the name of the current database after a connection is opened, or the database name specified in the connection string before the connection is opened.
        /// </summary>
        /// <returns>
        /// The name of the current database or the name of the database to be used after a connection is opened. The default value is an empty string.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string Database => this.database;

        /// <summary>
        /// Gets the name of the database server to which to connect.
        /// </summary>
        /// <returns>
        /// The name of the database server to which to connect. The default value is an empty string.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string DataSource
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected.
        /// </summary>
        /// <returns>
        /// The version of the database. The format of the string returned depends on the specific type of connection you are using.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException"><see cref="P:System.Data.Common.DbConnection.ServerVersion"/> was called while the returned Task was not completed and the connection was not opened after a call to <see cref="DbConnection.OpenAsync()"/>.</exception><filterpriority>2</filterpriority>
        public override string ServerVersion
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets a string that describes the state of the connection.
        /// </summary>
        /// <returns>
        /// The state of the connection. The format of the string returned depends on the specific type of connection you are using.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override ConnectionState State => ConnectionState.Open;

        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database. </param>
        public override void ChangeDatabase(string databaseName)
        {
            this.database = databaseName;
        }

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// </summary>
        /// <exception cref="T:System.Data.Common.DbException">The connection-level error that occurred while opening the connection. </exception><filterpriority>1</filterpriority>
        public override void Close()
        {
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the <see cref="P:System.Data.Common.DbConnection.ConnectionString"/>.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Open()
        {
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Creates and returns a <see cref="T:System.Data.Common.DbCommand"/> object associated with the current connection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbCommand"/> object.
        /// </returns>
        protected override DbCommand CreateDbCommand()
        {
            throw new InvalidOperationException();
        }
    }
}