// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShardingProxyWrappperActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Wraps sharding proxy into well known path
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
    using Akka.Event;

    using Autofac;

    /// <summary>
    /// Wraps sharding proxy into well known path
    /// </summary>
    public class ShardingProxyWrappperActor : ReceiveActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShardingProxyWrappperActor"/> class.
        /// </summary>
        /// <param name="container">
        /// The dependency resolver.
        /// </param>
        /// <param name="shardingConfig">
        /// The sharding config.
        /// </param>
        public ShardingProxyWrappperActor(IComponentContext container, Config shardingConfig)
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
                       "{Type}: initializing Sharding proxy manager on {PathString}",
                       typeof(NameSpaceActor).Name,
                       this.Self.Path.ToString());

            var shardingRegion = ClusterSharding.Get(Context.System)
                .StartProxy(
                    shardingTypeName,
                    role,
                    (IMessageExtractor)container.Resolve(messageExtractorType));

            this.Receive<object>(m => shardingRegion.Tell(m, this.Sender));
        }
    }
}