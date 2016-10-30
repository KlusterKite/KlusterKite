// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseConnectionManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to create database connections
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF
{
    using System.Data.Common;
    using System.Text.RegularExpressions;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class to create database connections
    /// </summary>
    public abstract class BaseConnectionManager
    {
        /// <summary>
        /// Checks for database existence. In case it is not - creates it
        /// </summary>
        /// <param name="connection">Opened database connection</param>
        /// <param name="databaseName">Database name to check</param>
        public abstract void CheckCreateDatabase(DbConnection connection, string databaseName);

        /// <summary>
        /// Creates connection to database
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Database connection</returns>
        /// <remarks>Don't forget to dispose it</remarks>
        public abstract DbConnection CreateConnection(string connectionString);

        /// <summary>
        /// Escapes stringed database name (or related object) in terms of current database system
        /// </summary>
        /// <param name="databaseName">Configured database name</param>
        /// <returns>Escaped database name</returns>
        [UsedImplicitly]
        public virtual string EscapeDatabaseName(string databaseName)
        {
            return Regex.Replace(databaseName, "[^\\w\\.]+", string.Empty);
        }

        /// <summary>
        /// Changes database in database connection
        /// </summary>
        /// <param name="connection">
        /// Opened database connection
        /// </param>
        /// <param name="databaseName">
        /// The database Name.
        /// </param>
        public virtual void SwitchDatabase(DbConnection connection, string databaseName)
        {
            connection.ChangeDatabase(databaseName);
        }
    }
}