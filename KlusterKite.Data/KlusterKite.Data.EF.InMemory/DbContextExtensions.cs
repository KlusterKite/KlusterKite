// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbContextExtensions.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Fixing the https://github.com/aspnet/EntityFramework/issues/6872#issuecomment-258025241
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.EF.InMemory
{
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.ValueGeneration;

    /// <summary>
    /// Fixing the <see href="https://github.com/aspnet/EntityFramework/issues/6872#issuecomment-258025241"/>
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Resets the current value generators
        /// </summary>
        /// <param name="context">The data context</param>
        public static void ResetValueGenerators(this DbContext context)
        {
            var cache = context.GetService<IValueGeneratorCache>();

            foreach (var keyProperty in context.Model.GetEntityTypes()
                .Select(e => e.FindPrimaryKey().Properties[0])
                .Where(p => p.ClrType == typeof(int)
                            && p.ValueGenerated == ValueGenerated.OnAdd))
            {
                var generator = (ResettableValueGenerator)cache.GetOrAdd(
                    keyProperty,
                    keyProperty.DeclaringEntityType,
                    (p, e) => new ResettableValueGenerator());

                generator.Reset();
            }
        }
    }
}