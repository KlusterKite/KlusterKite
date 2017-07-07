// --------------------------------------------------------------------------------------------------------------------
// <copyright file="mAYBEtESTS.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Tests of the <see cref="Maybe{T}" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Tests.Monads
{
    using System.Diagnostics.CodeAnalysis;

    using ClusterKit.Core.Monads;

    using Xunit;

    /// <summary>
    /// Tests of the <see cref="Maybe{T}"/>
    /// </summary>
    public class MaybeTests
    {
        /// <summary>
        /// Tests equality operators
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1216:NoValueFirstComparison", Justification = "Reviewed. Suppression is OK here.")]
        [Fact]
        public void EqualTests()
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.True(new Maybe<string>("test") == new Maybe<string>("test"));
            Assert.True(new Maybe<string>("test") == "test");
            Assert.True("test" == new Maybe<string>("test"));
            // ReSharper disable once EqualExpressionComparison
            Assert.False(new Maybe<string>("test") != new Maybe<string>("test"));
            Assert.False(new Maybe<string>("test") != "test");
            Assert.False("test" != new Maybe<string>("test"));

            // ReSharper disable once EqualExpressionComparison
            Assert.False(new Maybe<string>("test") == new Maybe<string>("test1"));
            Assert.False(new Maybe<string>("test") == "test1");
            Assert.False("test" == new Maybe<string>("test1"));
            // ReSharper disable once EqualExpressionComparison
            Assert.True(new Maybe<string>("test") != new Maybe<string>("test1"));
            Assert.True(new Maybe<string>("test") != "test1");
            Assert.True("test" != new Maybe<string>("test1"));

            Assert.True(new Maybe<string>(null) == null);
            Assert.True(null == new Maybe<string>(null));
            Assert.False(new Maybe<string>(null) != null);
            Assert.False(null != new Maybe<string>(null));

            Assert.False(new Maybe<string>("test") == null);
            Assert.False(null == new Maybe<string>("test"));
            Assert.True(new Maybe<string>("test") != null);
            Assert.True(null != new Maybe<string>("test"));
        }
    }
}