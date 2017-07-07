// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnSecurityLogType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The log record type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Attributes
{
    /// <summary>
    /// The log record type
    /// </summary>
    public enum EnSecurityLogType
    {
        /// <summary>
        /// Data read operation was successful
        /// </summary>
        DataReadGranted,

        /// <summary>
        /// Data create operation was successful
        /// </summary>
        DataCreateGranted,

        /// <summary>
        /// Data update / change operation was successful
        /// </summary>
        DataUpdateGranted,

        /// <summary>
        /// Data removal operation was successful
        /// </summary>
        DataDeleteGranted,

        /// <summary>
        /// Some uncategorized operation was successfully made
        /// </summary>
        OperationGranted,

        /// <summary>
        /// Authentication grant was successful
        /// </summary>
        AuthenticationGranted,

        /// <summary>
        /// Unsuccessful authentication attempt
        /// </summary>
        AuthenticationDenied,

        /// <summary>
        /// Attempt to make unauthorized operation
        /// </summary>
        OperationDenied,
    }
}