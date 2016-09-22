// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelNotFoundException.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The parcel was already removed from the server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects.Client
{
    using System;

    /// <summary>
    /// The parcel was already removed from the server
    /// </summary>
    public class ParcelNotFoundException : Exception
    {
    }
}