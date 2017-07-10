// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuidGraphType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Scalar graph type representing UID
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher.GraphTypes
{
    using System;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Types;

    /// <summary>
    /// Scalar graph type representing UID
    /// </summary>
    public class GuidGraphType : ScalarGraphType
    {
        /// <inheritdoc />
        public GuidGraphType()
        {
            this.Name = "Uid";
            this.Description = "The Uid / Guid representation";
        }

        /// <inheritdoc />
        public override object Serialize(object value)
        {
            return this.ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value)
        {
            Guid result;
            if (Guid.TryParse(value?.ToString() ?? string.Empty, out result))
            {
                return result;
            }

            return null;
        }

        /// <inheritdoc />
        public override object ParseLiteral(IValue value)
        {
            if (value is StringValue)
            {
                return this.ParseValue(((StringValue)value).Value);
            }

            return null;
        }
    }
}
