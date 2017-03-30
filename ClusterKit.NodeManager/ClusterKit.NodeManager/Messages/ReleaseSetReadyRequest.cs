// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSetReadyRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Moves <see cref="FSMBase.State{TS,TD}" /> from <see cref="Release.EnState.Draft" /> to <see cref="Release.EnState.Ready" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using Akka.Actor;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

    /// <summary>
    /// Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="Release.EnState.Draft"/> to <see cref="Release.EnState.Ready"/>
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ReleaseSetReadyRequest
    {
        /// <summary>
        /// Gets or sets the release id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}
