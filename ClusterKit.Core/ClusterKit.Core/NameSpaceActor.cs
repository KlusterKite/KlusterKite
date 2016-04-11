﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameSpaceActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster.Tools.Singleton;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;

    using Castle.Windsor;

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
        /// <param name="windsorContainer">
        /// Dependency resolver
        /// </param>
        public NameSpaceActor(IWindsorContainer windsorContainer)
        {
            InitChildActorsFromConfig(NameSpaceActor.Context, this.Self.Path, windsorContainer);
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

        /// <summary>
        /// Creates cluster sharding actor from config
        /// </summary>
        /// <param name="context">Current actor context (will create child actor)</param>
        /// <param name="actorConfig">Configuration to create from</param>
        /// <param name="windsorContainer">Dependency resolver</param>
        /// <param name="pathName">New actor's path name</param>
        private static void CreateShardingActor(
            IActorContext context,
            Config actorConfig,
            IWindsorContainer windsorContainer,
            string pathName)
        {
            context.ActorOf(Props.Create(() => new ShardingWrappperActor(windsorContainer, actorConfig)), pathName);
        }

        /// <summary>
        /// Creates cluster sharding proxy actor from config
        /// </summary>
        /// <param name="context">Current actor context (will create child actor)</param>
        /// <param name="actorConfig">Configuration to create from</param>
        /// <param name="windsorContainer">Dependency resolver</param>
        /// <param name="pathName">New actor's path name</param>
        private static void CreateShardingProxyActor(
            IActorContext context,
            Config actorConfig,
            IWindsorContainer windsorContainer,
            string pathName)
        {
            context.ActorOf(Props.Create(() => new ShardingProxyWrappperActor(windsorContainer, actorConfig)), pathName);
        }

        /// <summary>
        /// Creates simple actor from config
        /// </summary>
        /// <param name="context">Current actor context (will create child actor)</param>
        /// <param name="actorConfig">Configuration to create from</param>
        /// <param name="currentPath">Parent (current) actor path</param>
        /// <param name="pathName">New actor's path name</param>
        private static void CreateSimpleActor(
            IActorContext context,
            Config actorConfig,
            string currentPath,
            string pathName)
        {
            var childTypeName = actorConfig.GetString("type");
            if (string.IsNullOrWhiteSpace(childTypeName))
            {
                return;
            }

            var type = Type.GetType(childTypeName);
            if (type != null)
            {
                context.GetLogger()
                    .Info(
                        "{Type}: {NameSpaceName} initializing {ActorType} on {PathString}",
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        type.Name,
                        pathName);
                context.ActorOf(Context.System.DI().Props(type), pathName);
            }
            else
            {
                context.GetLogger()
                    .Error(
                        "{Type}: {ClassTypeString} was not found for actor {NameSpaceName}/{PathString}",
                        typeof(NameSpaceActor).Name,
                        childTypeName,
                        currentPath,
                        pathName);
            }
        }

        /// <summary>
        /// Creates cluster singleton actor from config
        /// </summary>
        /// <param name="context">Current actor context (will create child actor)</param>
        /// <param name="actorConfig">Configuration to create from</param>
        /// <param name="currentPath">Parent (current) actor path</param>
        /// <param name="pathName">New actor's path name</param>
        private static void CreateSingletonActor(
            IActorContext context,
            Config actorConfig,
            string currentPath,
            string pathName)
        {
            var childTypeName = actorConfig.GetString("type");
            if (string.IsNullOrWhiteSpace(childTypeName))
            {
                return;
            }

            var type = Type.GetType(childTypeName);
            if (type == null)
            {
                context.GetLogger()
                    .Error(
                        "{Type}: {ClassTypeString} was not found for actor {NameSpaceName}/{PathString}",
                        typeof(NameSpaceActor).Name,
                        childTypeName,
                        currentPath,
                        pathName);
                return;
            }

            var singletonName = actorConfig.GetString("singleton-name");
            if (string.IsNullOrEmpty(singletonName))
            {
                context.GetLogger()
                    .Error(
                        "{Type}: singleton-name was not defined for{NameSpaceName}/ {PathString}",
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        pathName);
                return;
            }

            var role = actorConfig.GetString("singleton-node-role");
            if (string.IsNullOrEmpty(role))
            {
                context.GetLogger()
                    .Error(
                        "{Type}: singleton-node-role was not defined for {NameSpaceName}/{PathString}",
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        pathName);
                return;
            }

            context.GetLogger()
                .Info(
                    "{Type}: {NameSpaceName} initializing singleton {SingletonName} of type {ActorType} on {PathString}",
                    typeof(NameSpaceActor).Name,
                    currentPath,
                    singletonName,
                    type.Name,
                    pathName);

            context.ActorOf(
                ClusterSingletonManager.Props(
                    context.System.DI().Props(type),
                    new ClusterSingletonManagerSettings(
                        singletonName,
                        role,
                        actorConfig.GetTimeSpan("removal-margin", TimeSpan.FromSeconds(1), false),
                        actorConfig.GetTimeSpan("handover-retry-interval", TimeSpan.FromSeconds(5), false))),
                pathName);
        }

        /// <summary>
        /// Creates cluster singleton proxy actor from config
        /// </summary>
        /// <param name="context">Current actor context (will create child actor)</param>
        /// <param name="actorConfig">Configuration to create from</param>
        /// <param name="currentPath">Parent (current) actor path</param>
        /// <param name="pathName">New actor's path name</param>
        private static void CreateSingletonProxyActor(
            IActorContext context,
            Config actorConfig,
            string currentPath,
            string pathName)
        {
            var singletonName = actorConfig.GetString("singleton-name");
            if (string.IsNullOrEmpty(singletonName))
            {
                context.GetLogger()
                    .Error(
                        "{Type}: singleton-name was not defined for {NameSpaceName}/{PathString}",
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        pathName);
                return;
            }

            var singletonManagerPath = actorConfig.GetString("singleton-path");
            if (string.IsNullOrEmpty(singletonName))
            {
                context.GetLogger()
                    .Error(
                        "{Type}: singleton-path was not defined for {NameSpaceName}/{PathString}",
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        pathName);
                return;
            }

            var role = actorConfig.GetString("singleton-node-role");
            if (string.IsNullOrEmpty(role))
            {
                context.GetLogger()
                    .Error(
                        "{Type}: singleton-node-role was not defined for {NameSpaceName}/{PathString}",
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        pathName);
                return;
            }

            context.GetLogger()
                .Info(
                    "{Type}: {NameSpaceName} initializing singleton proxy {SingletonManagerPath} / {SingletonName} for {PathString}",
                    typeof(NameSpaceActor).Name,
                    currentPath,
                    singletonManagerPath,
                    singletonName,
                    pathName);

            context.ActorOf(
                ClusterSingletonProxy.Props(
                    singletonManagerPath: singletonManagerPath,
                    settings:
                        new ClusterSingletonProxySettings(
                            singletonName,
                            role,
                            actorConfig.GetTimeSpan("singleton-identification-interval", TimeSpan.FromSeconds(1), false),
                            actorConfig.GetInt("buffer-size", 2048))),
                name: pathName);
        }

        /// <summary>
        /// Creates child actors according to current config
        /// </summary>
        /// <param name="context">Current actor's context</param>
        /// <param name="actorPath">Current actor's path</param>
        /// <param name="container">Dependency resolver</param>
        private static void InitChildActorsFromConfig(IActorContext context, ActorPath actorPath, IWindsorContainer container)
        {
            var config = Context.System.Settings.Config.GetConfig("akka.actor.deployment");
            if (config == null)
            {
                return;
            }

            var namespacePathElements = actorPath.Elements.Skip(1).ToList();
            foreach (var pair in config.AsEnumerable())
            {
                var key = pair.Key;

                var path = key.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Length != namespacePathElements.Count + 1)
                {
                    continue;
                }

                if (namespacePathElements.Where((t, i) => path[i] != t).Any())
                {
                    continue;
                }

                var actorConfig = config.GetConfig(key);
                EnActorType actorType;
                if (!Enum.TryParse(actorConfig.GetString("actor-type"), out actorType))
                {
                    actorType = EnActorType.Simple;
                }

                var currentPath = "/" + string.Join("/", namespacePathElements);

                switch (actorType)
                {
                    case EnActorType.Singleton:
                        CreateSingletonActor(context, actorConfig, currentPath, path.Last());
                        break;

                    case EnActorType.SingletonProxy:
                        CreateSingletonProxyActor(context, actorConfig, currentPath, path.Last());
                        break;

                    case EnActorType.Sharding:
                        CreateShardingActor(context, actorConfig, container, path.Last());
                        break;

                    case EnActorType.ShardingProxy:
                        CreateShardingProxyActor(context, actorConfig, container, path.Last());
                        break;

                    default:
                        CreateSimpleActor(context, actorConfig, currentPath, path.Last());
                        break;
                }
            }
        }
    }
}