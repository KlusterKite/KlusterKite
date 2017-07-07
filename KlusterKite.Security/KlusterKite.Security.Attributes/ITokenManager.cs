// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITokenManager.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Generic token manager. Provides full stack utilities to work with user authentication sessions and corresponding tokens
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Attributes
{
    using System.Threading.Tasks;

    /// <summary>
    /// Generic token manager. Provides full stack utilities to work with user authentication sessions and corresponding tokens
    /// </summary>
    public interface ITokenManager
    {
        /// <summary>
        /// Creates access token
        /// </summary>
        /// <param name="session">The authentication session</param>
        /// <returns>The access token</returns>
        Task<string> CreateAccessToken(AccessTicket session);

        /// <summary>
        /// Creates refresh token
        /// </summary>
        /// <param name="ticket">The authentication identity</param>
        /// <returns>The refresh token</returns>
        Task<string> CreateRefreshToken(RefreshTicket ticket);

        /// <summary>
        /// Validates access token
        /// </summary>
        /// <param name="token">The user access token</param>
        /// <returns>The user session for the valid token or null in other case</returns>
        Task<AccessTicket> ValidateAccessToken(string token);

        /// <summary>
        /// Validates refresh token
        /// </summary>
        /// <param name="token">The user refresh token</param>
        /// <returns>The authentication identity for the valid token or null in other case</returns>
        Task<RefreshTicket> ValidateRefreshToken(string token);

        /// <summary>
        /// Revokes access token
        /// </summary>
        /// <param name="token">The user access token</param>
        /// <returns>The success of the operation. The permission to revoke token should be checked elsewhere. Not all providers can revoke tokens</returns>
        Task<bool> RevokeAccessToken(string token);

        /// <summary>
        /// Revokes refresh token
        /// </summary>
        /// <param name="token">The user refresh token</param>
        /// <returns>The success of the operation. The permission to revoke token should be checked elsewhere. Not all providers can revoke tokens</returns>
        Task<bool> RevokeRefreshToken(string token);
    }
}