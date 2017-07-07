// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Converter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Provides converter methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;

    using Akka;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides converter methods
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Converts arguments to JSON object
        /// </summary>
        /// <param name="arguments">
        /// Arguments list
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The corresponding JSON
        /// </returns>
        public static JObject ToJson(this IEnumerable<Argument> arguments, ResolveFieldContext context)
        {
            var result = new JObject();
            foreach (var argument in arguments)
            {
                var value = ToJson(argument.Value, context);
                result.Add(argument.Name, value);
            }

            return result;
        }

        /// <summary>
        /// Converts <see cref="ObjectValue"/> to JSON object
        /// </summary>
        /// <param name="objectValue">
        /// The object value.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="JToken"/>.
        /// </returns>
        private static JToken ToJson(ObjectValue objectValue, ResolveFieldContext context)
        {
            var result = new JObject();
            foreach (var field in objectValue.ObjectFields)
            {
                var value = ToJson(field.Value, context);
                result.Add(field.Name, value);
            }

            return result;
        }

        /// <summary>
        /// Converts <see cref="ObjectValue"/> to JSON object
        /// </summary>
        /// <param name="value">
        /// The abstract value.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="JToken"/>.
        /// </returns>
        private static JToken ToJson(IValue value, ResolveFieldContext context)
        {
            return value.Match<JToken>()
                    .With<VariableReference>(r =>
                    {
                        var variableValue = context.Variables.ValueFor(r.Name);
                        var token = JToken.FromObject(variableValue);
                        return token;
                    })
                    .With<IntValue>(v => new JValue(v.Value))
                    .With<FloatValue>(v => new JValue(v.Value))
                    .With<StringValue>(v => new JValue(v.Value))
                    .With<DecimalValue>(v => new JValue(v.Value))
                    .With<FloatValue>(v => new JValue(v.Value))
                    .With<BooleanValue>(v => new JValue(v.Value))
                    .With<LongValue>(v => new JValue(v.Value))
                    .With<EnumValue>(v => new JValue(v.Name))
                    .With<ListValue>(v => new JArray(v.Values.Select(sv => ToJson(sv, context))))
                    .With<ObjectValue>(sv => ToJson(sv, context))
                    .ResultOrDefault(v => null);
        }
    }
}
