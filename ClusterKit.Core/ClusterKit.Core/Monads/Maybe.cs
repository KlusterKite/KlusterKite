// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Maybe.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic helper to make typed nulls
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Monads
{
    using JetBrains.Annotations;

    /// <summary>
    /// Generic helper to make typed nulls
    /// </summary>
    /// <typeparam name="T">End type</typeparam>
    public struct Maybe<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Maybe{T}"/> struct.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public Maybe(T value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this is not a null
        /// </summary>
        [UsedImplicitly]
        public bool HasValue => this.Value != null;

        /// <summary>
        /// Gets the value.
        /// </summary>
        private T Value { get; }

        /// <summary>
        /// Converts original object to wrapper
        /// </summary>
        /// <param name="obj">The original object</param>
        public static implicit operator Maybe<T>(T obj)
        {
            return new Maybe<T>(obj);
        }

        /// <summary>
        /// Converts wrapper to the original object
        /// </summary>
        /// <param name="obj">Wrapped object</param>
        public static implicit operator T(Maybe<T> obj)
        {
            return obj.Value;
        }

        /// <summary>
        /// Not equals for two wrappers
        /// </summary>
        /// <param name="left">Left wrapper</param>
        /// <param name="right">Right wrapper</param>
        /// <returns>Whether theese to wrapers are not equal</returns>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Equals for two wrappers
        /// </summary>
        /// <param name="left">Left wrapper</param>
        /// <param name="right">Right wrapper</param>
        /// <returns>Whether theese to wrapers are equal</returns>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Equals for two wrappers
        /// </summary>
        /// <param name="other">Wrapper to compare</param>
        /// <returns>Whether theese to wrapers are equal</returns>
        [UsedImplicitly]
        public bool Equals(Maybe<T> other)
        {
            if (this.Value == null && other.Value == null)
            {
                return true;
            }

            if (this.Value == null || other.Value == null)
            {
                return false;
            }

            return this.Value.Equals(other.Value);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            return obj is Maybe<T> && this.Equals((Maybe<T>)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Value?.GetHashCode() ?? 0;
        }
    }
}