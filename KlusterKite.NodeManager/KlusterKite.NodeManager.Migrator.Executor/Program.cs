// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The code launcher
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Migrator.Executor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Configuration;

    using Autofac;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Launcher.Utils;
    using ClusterKit.NodeManager.RemoteDomain;

    using JetBrains.Annotations;

    /// <summary>
    /// The code launcher
    /// </summary>
    [UsedImplicitly]
    public class Program
    {
        /// <summary>
        /// Gets the list of errors
        /// </summary>
        private static readonly List<MigrationLogRecord> Errors = new List<MigrationLogRecord>();

        /// <summary>
        /// Service main entry point
        /// </summary>
        public static void Main()
        {
            var builder = new ContainerBuilder();
            try
            {
                builder.RegisterInstallers();
            }
            catch (ReflectionTypeLoadException exception)
            {
                foreach (var loaderException in exception.LoaderExceptions)
                {
                    throw loaderException;
                }

                throw;
            }

            var providedConfiguration = File.ReadAllText("config.hocon");
            var config = BaseInstaller.GetStackedConfig(
                builder,
                ConfigurationFactory.ParseString(providedConfiguration));
            builder.RegisterInstance(config).As<Config>();
            BaseInstaller.RunComponentRegistration(builder, config);

            var migratorTypeNames = config.GetStringList("ClusterKit.NodeManager.Migrators");
            foreach (var typeName in migratorTypeNames)
            {
                var type = Type.GetType(typeName, false);
                if (type == null)
                {
                    Errors.Add(
                        new MigrationLogRecord
                            {
                                Type = EnMigrationLogRecordType.Error,
                                ErrorMessage = $"Migrator type {typeName} was not found",
                                MigratorTypeName = typeName
                            });
                    continue;
                }

                if (!type.GetTypeInfo().GetInterfaces().Contains(typeof(IMigrator)))
                {
                    Errors.Add(
                        new MigrationLogRecord
                            {
                                Type = EnMigrationLogRecordType.Error,
                                ErrorMessage = $"Type {typeName} doesn't implement IMigrator",
                                MigratorTypeName = typeName
                            });
                    continue;
                }

                builder.RegisterType(type).As<IMigrator>();
            }

            var context = builder.Build();
            Console.WriteLine(ProcessHelper.EOF);

            MigrationCollector collector;
            using (var standardInput = Console.OpenStandardInput())
            {
                collector = new StreamReader(standardInput).Receive() as MigrationCollector;
            }

            if (collector == null)
            {
                throw new InvalidOperationException("Received object is not a MigrationCollector");
            }

            collector.Errors.AddRange(Errors);
            if (Errors.Count == 0)
            {
                collector.Execute(context);
            }

            Console.Out.Send(collector);
        }
    }
}