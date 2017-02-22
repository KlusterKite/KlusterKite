// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwapVisitor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper to constract sorting expression
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API.Resolvers
{
    using System.Linq.Expressions;

    /// <summary>
    /// Helper to construct sorting expression
    /// </summary>
    /// <remarks>
    /// Original: <see href="http://stackoverflow.com/questions/9132479/combine-lambda-expressions"/>
    /// </remarks>
    public class SwapVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The left expression
        /// </summary>
        private readonly Expression from;

        /// <summary>
        /// The right expression
        /// </summary>
        private readonly Expression to;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapVisitor"/> class.
        /// </summary>
        /// <param name="from">
        /// The left expression.
        /// </param>
        /// <param name="to">
        /// The right expression.
        /// </param>
        public SwapVisitor(Expression from, Expression to)
        {
            this.@from = @from;
            this.to = to;
        }

        /// <inheritdoc />
        public override Expression Visit(Expression node)
        {
            return node == this.@from ? this.to : base.Visit(node);
        }
    }
}