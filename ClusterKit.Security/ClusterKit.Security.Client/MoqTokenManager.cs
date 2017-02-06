// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoqTokenManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Token manager for test purposes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// Token manager for test purposes
    /// </summary>
    public class MoqTokenManager : ITokenManager
    {
        /// <summary>
        /// The list of stored access tickets
        /// </summary>
        private readonly Dictionary<string, AccessTicket> storedAccessTickets = new Dictionary<string, AccessTicket>();

        /// <summary>
        /// The list of stored refresh tickets
        /// </summary>
        private readonly Dictionary<string, RefreshTicket> storedRefreshTickets = new Dictionary<string, RefreshTicket>();

        /// <summary>
        /// Gets or sets virtual current time to test token invalidation
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset? CurrentTime { get; set; }

        /// <inheritdoc />
        public Task<string> CreateAccessToken(AccessTicket session)
        {
            var token = Guid.NewGuid().ToString("N");
            this.storedAccessTickets[token] = session;
            return Task.FromResult(token);
        }

        /// <inheritdoc />
        public Task<AccessTicket> ValidateAccessToken(string token)
        {
            AccessTicket ticket;
            if (!this.storedAccessTickets.TryGetValue(token, out ticket))
            {
                return Task.FromResult<AccessTicket>(null);
            }

            if (ticket.Expiring.HasValue && ticket.Expiring.Value < (this.CurrentTime ?? DateTimeOffset.Now))
            {
                return Task.FromResult<AccessTicket>(null);
            }

            return Task.FromResult(ticket);
        }

        /// <inheritdoc />
        public Task<bool> RevokeAccessToken(string token)
        {
            AccessTicket ticket;
            if (!this.storedAccessTickets.TryGetValue(token, out ticket))
            {
                return Task.FromResult(false);
            }

            this.storedAccessTickets.Remove(token);
            return
                Task.FromResult(
                    !ticket.Expiring.HasValue || ticket.Expiring.Value > (this.CurrentTime ?? DateTimeOffset.Now));
        }

        /// <inheritdoc />
        public Task<string> CreateRefreshToken(RefreshTicket ticket)
        {
            var token = Guid.NewGuid().ToString("N");
            this.storedRefreshTickets[token] = ticket;
            return Task.FromResult(token);
        }

        /// <inheritdoc />
        public Task<RefreshTicket> ValidateRefreshToken(string token)
        {
            RefreshTicket ticket;
            if (!this.storedRefreshTickets.TryGetValue(token, out ticket))
            {
                return Task.FromResult<RefreshTicket>(null);
            }

            if (ticket.Expiring.HasValue && ticket.Expiring.Value < (this.CurrentTime ?? DateTimeOffset.Now))
            {
                return Task.FromResult<RefreshTicket>(null);
            }

            return Task.FromResult(ticket);
        }

        /// <inheritdoc />
        public Task<bool> RevokeRefreshToken(string token)
        {
            RefreshTicket ticket;
            if (!this.storedRefreshTickets.TryGetValue(token, out ticket))
            {
                return Task.FromResult(false);
            }

            this.storedRefreshTickets.Remove(token);
            return
                Task.FromResult(
                    !ticket.Expiring.HasValue || ticket.Expiring.Value > (this.CurrentTime ?? DateTimeOffset.Now));
        }
    }
}
