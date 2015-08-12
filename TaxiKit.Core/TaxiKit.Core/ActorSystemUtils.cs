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

    using Akka.Actor;
    using Akka.DI.Core;

    /// <summary>
    /// Utilities to work with <seealso cref="ActorSystem"/>
    /// </summary>
    public static class ActorSystemUtils
    {
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