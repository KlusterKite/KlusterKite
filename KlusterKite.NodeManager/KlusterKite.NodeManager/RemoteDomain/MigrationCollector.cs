// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationCollector.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MigrationCollector type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.RemoteDomain
{
    using System;
    using System.Collections.Generic;

    using Autofac;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Migrator;

    /// <summary>
    /// Base class to launch <see cref="IMigrator"/> in remote domain
    /// </summary>
    public abstract class MigrationCollector
    {
        /// <summary>
        /// Gets the list of errors
        /// </summary>
        public List<MigrationLogRecord> Errors { get; } = new List<MigrationLogRecord>();

        /// <summary>
        /// Gets or sets the result
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Executes the collector
        /// </summary>
        /// <param name="componentContext">
        /// The component context.
        /// </param>
        public void Execute(IComponentContext componentContext)
        {
            try
            {
                this.Result = this.GetResult(componentContext);
            }
            catch (Exception e)
            {
                this.Errors.Add(new MigrationLogRecord { Type = EnMigrationLogRecordType.Error, ErrorMessage = e.Message, Exception = e });
            }
        }

        /// <summary>
        /// Gets the list of migrators
        /// </summary>
        /// <param name="componentContext">
        /// The component context.
        /// </param>
        /// <returns>
        /// The list of migrators
        /// </returns>
        protected IEnumerable<IMigrator> GetMigrators(IComponentContext componentContext)
        {
            return componentContext.Resolve<IEnumerable<IMigrator>>();
        }

        /// <summary>
        /// Creates the result value
        /// </summary>
        /// <param name="componentContext">
        /// The component context.
        /// </param>
        /// <returns>The result</returns>
        protected abstract object GetResult(IComponentContext componentContext);
    }

    /// <summary>
    /// The migration collector.
    /// </summary>
    /// <typeparam name="T">
    /// The expected result type
    /// </typeparam>
    // ReSharper disable once StyleCop.SA1402
    public abstract class MigrationCollector<T> : MigrationCollector where T : class
    {
        /// <summary>
        /// Gets or sets the result
        /// </summary>
        public new T Result
        {
            get => base.Result as T;

            set => base.Result = value;
        }

        /// <inheritdoc />
        protected override object GetResult(IComponentContext componentContext)
        {
            return this.GetTypedResult(componentContext);
        }

        /// <summary>
        /// Creates the result value
        /// </summary>
        /// <param name="componentContext">
        /// The component context.
        /// </param>
        /// <returns>The result</returns>
        protected abstract T GetTypedResult(IComponentContext componentContext);
    }
}