// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageNotFoundException.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The exception of missing requested package in repository
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Launcher.Utils.Exceptions
{
    using System;

    /// <summary>
    /// The exception of missing requested package in repository
    /// </summary>
    public class PackageNotFoundException : Exception
    {
        /// <inheritdoc />
        public PackageNotFoundException(string message)
            : base(message)
        {
        }
    }
}
