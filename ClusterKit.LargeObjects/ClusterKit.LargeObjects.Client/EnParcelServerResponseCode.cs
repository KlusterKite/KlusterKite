// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnParcelServerResponseCode.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of possible response codes from parcels server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects.Client
{
    /// <summary>
    /// The list of possible response codes from parcels server
    /// </summary>
    public enum EnParcelServerResponseCode : byte
    {
        /// <summary>
        /// The request is ok
        /// </summary>
        Ok = 0x00,

        /// <summary>
        /// The request could not be parsed
        /// </summary>
        BadRequest = 0x01,

        /// <summary>
        /// The specified parcel was not found
        /// </summary>
        NotFound = 0x02,
    }
}
