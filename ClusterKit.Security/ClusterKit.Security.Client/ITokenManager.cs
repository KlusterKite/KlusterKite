// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITokenManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic token manager. Provides full stack utilities to work with user authentication sessions and corresponding tokens
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System.Threading.Tasks;

    /// <summary>
    /// Generic token manager. Provides full stack utilities to work with user authentication sessions and corresponding tokens
    /// </summary>
    public interface ITokenManager
    {
        /// <summary>
        /// Creates token for the user sessions
        /// </summary>
        /// <param name="session">The user session</param>
        /// <returns>The access token</returns>
        Task<string> CreateAccessToken(UserSession session);

        /// <summary>
        /// Validates access token
        /// </summary>
        /// <param name="token">The user access token</param>
        /// <returns>The user session for the valid token or null in other case</returns>
        Task<UserSession> ValidateAccessToken(string token);
    }
}