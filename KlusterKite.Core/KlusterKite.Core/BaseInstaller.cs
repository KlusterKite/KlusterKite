﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseInstaller.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to install KlusterKite plugin components
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Configuration;

    using Autofac;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class to install KlusterKite plugin components
    /// </summary>
    [UsedImplicitly]
    public abstract class BaseInstaller
    {
        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles node role functionality
        /// </summary>
        [UsedImplicitly]
        protected const decimal PriorityClusterRole = 100M;

        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles other plugins functionality
        /// </summary>
        [UsedImplicitly]
        protected const decimal PrioritySharedLib = 10M;

        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles unit tests
        /// </summary>
        [UsedImplicitly]
        protected const decimal PriorityTest = 100M;

        /// <summary>
        /// Every time <seealso cref="Install"/> called, installer register itself here
        /// </summary>
        private static readonly Dictionary<ContainerBuilder, List<BaseInstaller>> RegisteredInstallers =
            new Dictionary<ContainerBuilder, List<BaseInstaller>>();

        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected abstract decimal AkkaConfigLoadPriority { get; }

        /// <summary>
        /// Gets the list of all registered installers
        /// </summary>
        /// <param name="container">
        /// The windsor builder.
        /// </param>
        /// <returns>
        /// the list of all registered installers
        /// </returns>
        public static IList<BaseInstaller> GetRegisteredBaseInstallers(ContainerBuilder container)
        {
            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return new List<BaseInstaller>();
            }

            return list;
        }

        /// <summary>
        /// Generates overall akka config from all registered modules (with respect to external provided configuration file)
        /// </summary>
        /// <param name="container">
        /// The windsor builder.
        /// </param>
        /// <param name="config">
        /// Top level config
        /// </param>
        /// <returns>
        /// Akka and system configuration
        /// </returns>
        public static Config GetStackedConfig(ContainerBuilder container, Config config)
        {
            Serilog.Log.Information("KlusterKite starting plugin manager");

            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return config;
            }

            var rolesList = list.SelectMany(i => i.GetRoles()).Distinct().Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => $"\"{r.Replace("\"", "\\\"")}\"").ToList();

            if (rolesList.Any())
            {
                config = config.WithFallback(
                    ConfigurationFactory.ParseString($"akka.cluster.roles = [{string.Join(", ", rolesList)}]"));
            }

            return list.OrderByDescending(i => i.AkkaConfigLoadPriority).Aggregate(
                config,
                (current, installer) => current.WithFallback(installer.GetAkkaConfig()));
        }

        /// <summary>
        /// Runs all registered installers 
        /// <seealso cref="PostStart"/>
        /// </summary>
        /// <param name="container">
        /// The builder builder
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        public static void RunPostStart(ContainerBuilder container, IComponentContext context)
        {
            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return;
            }

            foreach (var installer in list.OrderBy(l => l.AkkaConfigLoadPriority))
            {
                installer.PostStart(context);
            }
        }

        /// <summary>
        /// Runs all registered installers <seealso cref="PreCheck"/>
        /// </summary>
        /// <param name="container">
        /// The windsor builder.
        /// </param>
        /// <param name="config">Full akka config</param>
        public static void RunPreCheck(ContainerBuilder container, Config config)
        {
            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return;
            }

            foreach (var installer in list.OrderBy(l => l.AkkaConfigLoadPriority))
            {
                installer.PreCheck(config);
            }
        }

        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        public static void RunComponentRegistration([NotNull] ContainerBuilder builder, [NotNull] Config config)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            List<BaseInstaller> installers;
            if (!RegisteredInstallers.TryGetValue(builder, out installers))
            {
                throw new InvalidOperationException("builder is not registered");
            }

            foreach (var installer in installers)
            {
                installer.RegisterComponents(builder, config);
                builder.RegisterInstance(installer).As<BaseInstaller>();
            }
        }

        /// <summary>
        /// Reads texts resource
        /// </summary>
        /// <param name="assembly">The resource containing assembly</param>
        /// <param name="resourceName">The resource name</param>
        /// <returns>The resource contents</returns>
        public static string ReadTextResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">The builder.</param>
        public void Install([NotNull] ContainerBuilder container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            lock (RegisteredInstallers)
            {
                if (!RegisteredInstallers.ContainsKey(container))
                {
                    RegisteredInstallers[container] = new List<BaseInstaller>();
                }

                if (RegisteredInstallers[container].Contains(this))
                {
                    return;
                }

                RegisteredInstallers[container].Add(this);
            }
        }

        /// <summary>
        /// Should check the config and environment for possible errors.
        /// If any found, shod throw the exception to prevent node from starting.
        /// </summary>
        /// <param name="config">Full akka config</param>
        /// <exception cref="Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public virtual void PreCheck(Config config)
        {
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected abstract Config GetAkkaConfig();

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected virtual IEnumerable<string> GetRoles()
        {
            return new string[0];
        }

        /// <summary>
        /// This method will be run after service start.
        /// Methods are run in 
        /// <seealso cref="AkkaConfigLoadPriority"/>
        /// order.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        protected virtual void PostStart(IComponentContext context)
        {
        }

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">
        /// The builder.
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        protected abstract void RegisterComponents(ContainerBuilder container, Config config);
    }
}