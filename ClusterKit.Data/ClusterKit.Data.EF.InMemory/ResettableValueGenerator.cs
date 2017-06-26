// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResettableValueGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Fixing the <see href="https://github.com/aspnet/EntityFramework/issues/6872#issuecomment-258025241" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.InMemory
{
    using System.Threading;

    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.ValueGeneration;

    /// <summary>
    /// Fixing the <see href="https://github.com/aspnet/EntityFramework/issues/6872#issuecomment-258025241"/>
    /// </summary>
    public class ResettableValueGenerator : ValueGenerator<int>
    {
        /// <summary>
        /// The current value
        /// </summary>
        private int current;

        /// <inheritdoc />
        public override bool GeneratesTemporaryValues { get; } = false;

        /// <inheritdoc />
        public override int Next(EntityEntry entry)
            => Interlocked.Increment(ref this.current);

        /// <inheritdoc />
        public void Reset() => this.current = 0;
    }
}