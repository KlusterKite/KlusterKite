// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorSystemUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Utilities to work with ActorSystem
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.DI.Core;

    using Autofac;
#if CORECLR
        using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.PlatformAbstractions;
#endif

    /// <summary>
    /// Utilities to work with <seealso cref="ActorSystem"/>
    /// </summary>
    public static class ActorSystemUtils
    {
        /// <summary>
        /// Scans main application directory for libraries and <see cref="BaseInstaller"/> in them and installs them
        /// </summary>
        /// <param name="container">
        /// Current windsor container
        /// </param>
        /// <param name="installAssemblies">
        /// A value indicating whether this should scan current directory and load assemblies
        /// </param>
        public static void RegisterInstallers(this ContainerBuilder container, bool installAssemblies = true)
        {
            if (installAssemblies)
            {
#if APPDOMAIN
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#elif CORECLR
                var dir = PlatformServices.Default.Application.ApplicationBasePath;
#endif
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    var files = Directory.GetFiles(dir, "*.dll");
                    foreach (var file in files)
                    {
                        try
                        {
#if APPDOMAIN
                            Assembly.LoadFrom(file);
#elif CORECLR
                            AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
#else
#error Not implemented method
                            throw new NotImplementedException();
#endif
                        }
                        catch
                        {
                            // ignore - not all dll are loadable
                        }
                    }
                }
            }

            Console.WriteLine(@"Assemblies loaded");

            try
            {
                foreach (var type in GetLoadedAssemblies().Where(a => !a.IsDynamic).SelectMany(a => a.GetTypes())
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(BaseInstaller))).Where(
                        t => !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().IsGenericTypeDefinition
                             && t.GetConstructor(new Type[0]) != null))
                {
                    try
                    {
                        var installer = (BaseInstaller)Activator.CreateInstance(type);
                        installer.Install(container);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
            }
            catch (ReflectionTypeLoadException loadException)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!");
                foreach (var loaderException in loadException.LoaderExceptions)
                {
                    Console.WriteLine(loaderException.Message);
                }

                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!");
                throw;
            }

            Console.WriteLine(@"Assemblies installed");
        }

        /// <summary>
        /// Parses hocon configuration of actor system and starts <seealso cref="NameSpaceActor"/> that was defined in it
        /// </summary>
        /// <param name="sys">The actor system</param>
        public static void StartNameSpaceActorsFromConfiguration(this ActorSystem sys)
        {
            var config = sys.Settings.Config.GetConfig("akka.actor.deployment");
            if (config == null)
            {
                return;
            }

            foreach (var pair in config.AsEnumerable())
            {
                var key = pair.Key;
                var path = key.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Length != 1)
                {
                    continue;
                }

                var isNameSpace = config.GetConfig(key).GetBoolean("IsNameSpace");
                if (isNameSpace)
                {
                    sys.ActorOf(sys.DI().Props(typeof(NameSpaceActor)), path[0]);
                    sys.Log.Info(
                        "{Type}: starting namespace {NameSpaceName}",
                        typeof(ActorSystemUtils).Name,
                        path[0]);
                }
            }
        }

        /// <summary>
        /// Gets the list of loaded assemblies
        /// </summary>
        /// <returns>The list of loaded assemblies</returns>
        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
#if APPDOMAIN
            return AppDomain.CurrentDomain.GetAssemblies();
#elif CORECLR 
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
                catch
                {
                    //do nothing can't if can't load assembly
                }
            }
            return assemblies;
#else
#warning Method not implemented
            throw new NotImplementedException();
#endif
        }
    }
}