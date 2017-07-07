// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSetObsoleteRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Moves <see cref="FSMBase.State{TS,TD}" /> from <see cref="Release.EnReleaseState.Ready" /> to <see cref="Release.EnReleaseState.Obsolete" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using Akka.Actor;

    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="EnReleaseState.Ready"/> to <see cref="EnReleaseState.Obsolete"/>
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ReleaseSetObsoleteRequest
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