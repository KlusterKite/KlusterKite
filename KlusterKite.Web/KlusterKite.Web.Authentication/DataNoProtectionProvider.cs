// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataNoProtectionProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the DataNoProtectionProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authentication
{
    using Microsoft.AspNetCore.DataProtection;

    /// <summary>
    /// Substitute for data protection as data protection is provided by <see cref="KlusterKite.Security.Attributes.ITokenManager"/>
    /// </summary>
    public class DataNoProtectionProvider : IDataProtectionProvider
    {
        /// <inheritdoc />
        public IDataProtector CreateProtector(string purpose)
        {
            return new DataNotProtector();
        }

        /// <summary>
        /// Substitute for data protection as data protection is provided by <see cref="KlusterKite.Security.Attributes.ITokenManager"/>
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