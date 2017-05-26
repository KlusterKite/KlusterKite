// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;

    using Serilog;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
        /// <summary>
        /// Entity framework driver installer
        /// </summary>
        private BaseEntityFrameworkInstaller installer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// </summary>
        public Installer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// </summary>
        /// <param name="installer">Entity framework driver installer</param>
        public Installer(BaseEntityFrameworkInstaller installer)
        {
            this.installer = installer;
        }

        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        // ReSharper disable once ArrangeStaticMemberQualifier
        protected override decimal AkkaConfigLoadPriority => PrioritySharedLib;

        /// <summary>
        /// Should check the config and environment for possible errors.
        /// If any found, shod throw the exception to prevent node from starting.
        /// </summary>
        /// <param name="config">Full akka config</param>
        /// <exception cref="Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public override void PreCheck(Config config)
        {
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.Empty;

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
            if (this.installer == null)
            {
                Log.Information("Registering EF endpoint driver");
                try
                {
                    var installerTypes =
                        AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => t.IsSubclassOf(typeof(BaseEntityFrameworkInstaller)))
                            .ToList();

                    if (installerTypes.Count > 1)
                    {
                        throw new ConfigurationException(
                            $"There should be only one BaseEntityFrameworkInstaller, but found \n{string.Join(", \n", installerTypes.Select(t => t.FullName))}");
                    }

                    if (installerTypes.Count == 0)
                    {
                        throw new ConfigurationException("There is no BaseEntityFrameworkInstaller");
                    }

                    this.installer =
                        (BaseEntityFrameworkInstaller)
                        installerTypes.Single().GetConstructor(new Type[0])?.Invoke(new object[0]);
                    if (this.installer == null)
                    {
                        throw new InvalidOperationException();
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    foreach (var le in e.LoaderExceptions.Take(30))
                    {
                        Log.Logger.Error($"{le.Message}");
                    }

                    throw;
                }
            }

            if (this.installer != null)
            {
                // DbConfiguration.SetConfiguration(this.installer.GetConfiguration());
                var baseConnectionManager = this.installer.CreateConnectionManager();

                container.Register(
                    Component.For<BaseConnectionManager>().Instance(baseConnectionManager).LifestyleSingleton());
            }
        }
    }
}