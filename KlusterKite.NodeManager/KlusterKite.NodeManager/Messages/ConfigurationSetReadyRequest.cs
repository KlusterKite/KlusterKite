// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationSetReadyRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="EnConfigurationState.Draft"/> to <see cref="EnConfigurationState.Ready"/>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using Akka.Actor;
    using Akka.Routing;

    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="EnConfigurationState.Draft"/> to <see cref="EnConfigurationState.Ready"/>
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ConfigurationSetReadyRequest : IConsistentHashable
    {
        /// <summary>
        /// Gets or sets the configuration id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }

        /// <inheritdoc />
        public object ConsistentHashKey => this.Id;
    }
}