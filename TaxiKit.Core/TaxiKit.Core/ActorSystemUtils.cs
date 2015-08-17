// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorSystemUtils.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Utilities to work with <seealso cref="ActorSystem" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.DI.Core;

    using Castle.Facilities.TypedFactory;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    /// <summary>
    /// Utilities to work with <seealso cref="ActorSystem"/>
    /// </summary>
    public static class ActorSystemUtils
    {
        /// <summary>
        /// Scans main application directory for libraries and windsor installers in them and installs them
        /// </summary>
        /// <param name="container">Current windsor container</param>
        public static void RegisterWindsorInstallers(this IWindsorContainer container)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                var libs = Directory.GetFiles(dir, "*.dll");
                foreach (var lib in libs)
                {
                    Assembly.LoadFrom(lib);
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                try
                {
                    container.Install(FromAssembly.Instance(assembly));
                }
                catch (Exception)
                {
                }
            }
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
                    sys.ActorOf(sys.DI().Props<NameSpaceActor>(), path[0]);
                    sys.Log.Info(
                        "{Type}: starting namespace {NameSpaceName}",
                        typeof(ActorSystemUtils).Name,
                        path[0]);
                }
            }
        }
    }
}