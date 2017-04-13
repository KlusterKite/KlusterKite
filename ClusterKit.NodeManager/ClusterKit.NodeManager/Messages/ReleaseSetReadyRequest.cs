// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSetReadyRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Moves <see cref="FSMBase.State{TS,TD}" /> from <see cref="Release.EnReleaseState.Draft" /> to <see cref="Release.EnReleaseState.Ready" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using Akka.Actor;
    using Akka.Routing;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Attributes;

    /// <summary>
    /// Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="EnReleaseState.Draft"/> to <see cref="EnReleaseState.Ready"/>
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ReleaseSetReadyRequest : IConsistentHashable
    {
        /// <summary>
        /// Gets or sets the release id
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