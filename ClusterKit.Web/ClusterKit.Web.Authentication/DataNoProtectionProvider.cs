// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataNoProtectionProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the DataNoProtectionProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using Microsoft.AspNetCore.DataProtection;

    /// <summary>
    /// Substitute for data protection as data protection is provided by <see cref="ClusterKit.Security.Attributes.ITokenManager"/>
    /// </summary>
    public class DataNoProtectionProvider : IDataProtectionProvider
    {
        /// <inheritdoc />
        public IDataProtector CreateProtector(string purpose)
        {
            return new DataNotProtector();
        }

        /// <summary>
        /// Substitute for data protection as data protection is provided by <see cref="ClusterKit.Security.Attributes.ITokenManager"/>
        /// </summary>
        private class DataNotProtector : IDataProtector
        {
            /// <inheritdoc />
            public IDataProtector CreateProtector(string purpose)
            {
                return this;
            }

            /// <inheritdoc />
            public byte[] Protect(byte[] userData)
            {
                return userData;
            }

            /// <inheritdoc />
            public byte[] Unprotect(byte[] protectedData)
            {
                return protectedData;
            }
        }
    }
}