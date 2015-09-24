// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameSpaceActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.Linq;

    using Akka.Actor;
    using Akka.DI.Core;
    using Akka.Event;

    using JetBrains.Annotations;

    /// <summary>
    /// Actor to provide namespace in actors tree. Usually used only once in library
    /// </summary>
    [UsedImplicitly]
    public class NameSpaceActor : UntypedActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameSpaceActor"/> class.
        /// </summary>
        public NameSpaceActor()
        {
            var config = Context.System.Settings.Config.GetConfig("akka.actor.deployment");
            if (config == null)
            {
                return;
            }

            var namespacePath = this.Self.Path.Elements.Skip(1).ToList();

            foreach (var pair in config.AsEnumerable())
            {
                var key = pair.Key;
                var path = key.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Length != namespacePath.Count + 1)
                {
                    continue;
                }

                if (namespacePath.Where((t, i) => path[i] != t).Any())
                {
                    continue;
                }

                var childTypeName = config.GetConfig(key).GetString("type");
                if (string.IsNullOrWhiteSpace(childTypeName))
                {
                    continue;
                }

                var type = Type.GetType(childTypeName);
                if (type != null)
                {
                    Context.GetLogger().Info(
                        "{Type}: {NameSpaceName} initializing {ActorType} on {PathString}",
                        this.GetType().Name,
                        "/" + string.Join("/", namespacePath),
                        type.Name,
                        path.Last());
                    Context.ActorOf(Context.System.DI().Props(type), path.Last());
                }
                else
                {
                    Context.GetLogger().Error(
                        "{Type}: {ClassTypeString} was not found for actor {PathString}",
                        this.GetType().Name,
                        childTypeName,
                        key);
                }
            }
        }

        /// <summary>
        /// The on receive.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        protected override void OnReceive(object message)
        {
            this.Unhandled(message);
        }
    }
}