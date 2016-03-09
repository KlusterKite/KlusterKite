namespace ClusterKit.Core.EF
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;

    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Serilog;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
        private BaseEntityFrameworkInstaller installer;

        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected override decimal AkkaConfigLoadPriority => BaseInstaller.PrioritySharedLib;

        /// <summary>
        /// Should check the config and environment for possible erorrs.
        /// If any found, shod throw the exception to prevent node from starting.
        /// </summary>
        /// <param name="config">Full akka config</param>
        /// <exception cref="Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public override void PreCheck(Config config)
        {
            try
            {
                var installerType =
                    AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Single(t => t.IsSubclassOf(typeof(BaseEntityFrameworkInstaller)));

                this.installer = (BaseEntityFrameworkInstaller)installerType.GetConstructor(new Type[0])?.Invoke(new object[0]);
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
            }
            catch (InvalidOperationException)
            {
                throw new ConfigurationException("There should be exactly one EntityFrameworkInstaller in plugins with public parameterless construcot");
            }

            if (this.installer != null)
            {
                DbConfiguration.SetConfiguration(installer.GetConfiguration());
            }
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString("{}");

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
            if (this.installer != null)
            {
                container.Register(
                    Component.For<BaseConnectionManager>().Instance(this.installer.CreateConnectionManager()));
            }
        }
    }
}