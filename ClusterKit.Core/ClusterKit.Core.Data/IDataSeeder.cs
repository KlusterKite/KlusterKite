// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataSeeder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Objects used to fill empty database with start data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Data
{
    using JetBrains.Annotations;

    /// <summary>
    /// Objects used to fill empty database with start data
    /// </summary>
    /// <typeparam name="TContext">The type of data context</typeparam>
    [UsedImplicitly]
    public interface IDataSeeder<TContext>
    {
        /// <summary>
        /// Checks database for emptiness and fills with data
        /// </summary>
        /// <param name="context">Current opened context</param>
        void Seed(TContext context);
    }
}