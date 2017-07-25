// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShardingWrappperActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Wraps sharding into well known path
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core
{
    using System;
    using System.Linq;
#if CORECLR
    using System.Reflection;
#endif

    using Akka.Actor;
    using Akka.Cluster.Sharding;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;

    using Autofac;

    /// <summary>
    /// Wraps sharding into well known path
    /// </summary>
    public class ShardingWrappperActor : ReceiveActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShardingWrappperActor"/> class.
        /// </summary>
        /// <param name="container">
        /// The dependency resolver.
        /// </param>
        /// <param name="shardingConfig">
        /// The sharding config.
        /// </param>
        public ShardingWrappperActor(IComponentContext container, Config shardingConfig)
        {
            var shardingTypeName = shardingConfig.GetString("type-name");
            if (string.IsNullOrWhiteSpace(shardingTypeName))
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: type-name was not defined for actor {PathString}",
                        this.GetType().Name,
                        this.Self.Path.ToString());
                return;
            }

            var role = shardingConfig.GetString("role");
            if (string.IsNullOrWhiteSpace(role))
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: role was not defined for actor {PathString}",
                        this.GetType().Name,
                        this.Self.Path.ToString());
                return;
            }

            var childTypeName = shardingConfig.GetString("type");
            if (string.IsNullOrWhiteSpace(childTypeName))
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: Type was not defined for actor {PathString}",
                        this.GetType().Name,
                        this.Self.Path.ToString());
                return;
            }

            var type = Type.GetType(childTypeName);
            if (type == null)
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: {ClassTypeString} was not found for actor{PathString}",
                        this.GetType().Name,
                        childTypeName,
                        this.Self.Path.ToString());
                return;
            }

            var messageExtractorTypeName = shardingConfig.GetString("message-extractor");
            if (string.IsNullOrWhiteSpace(messageExtractorTypeName))
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: message-extractor was not defined for actor {PathString}",
                        this.GetType().Name,
                        this.Self.Path.ToString());
                return;
            }

            var messageExtractorType = Type.GetType(messageExtractorTypeName);
            if (messageExtractorType == null)
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: {ClassTypeString} was not found for actor {PathString}",
                        this.GetType().Name,
                        messageExtractorTypeName,
                        this.Self.Path.ToString());
                return;
            }

            if (messageExtractorType.GetInterfaces().All(t => t != typeof(IMessageExtractor)))
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: {ClassTypeString} defined in {PathString} does not implements IMessageExtractor",
                        this.GetType().Name,
                        messageExtractorTypeName,
                        this.Self.Path.ToString());
                return;
            }

            Context.GetLogger()
                   .Info(
                       "{Type}: initializing Sharding manager on {PathString}",
                       typeof(NameSpaceActor).Name,
                       this.Self.Path.ToString());

            var shardingRegion = ClusterSharding.Get(Context.System)
                .Start(
                    shardingTypeName,
                    Context.System.DI().Props(type),
                    ClusterShardingSettings.Create(Context.System).WithRole(role),
                    (IMessageExtractor)container.Resolve(messageExtractorType));

            this.Receive<object>(m => shardingRegion.Tell(m, this.Sender));
        }
    }
}