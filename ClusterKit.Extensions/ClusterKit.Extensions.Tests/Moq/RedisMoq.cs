// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisMoq.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Extensions.Tests.Moq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    ///     Redis interaction moq
    /// </summary>
    public class RedisMoq : IDatabase
    {
        /// <summary>
        /// moq redis storage
        /// </summary>
        private ConcurrentDictionary<string, object> storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisMoq"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        public RedisMoq(ConcurrentDictionary<string, object> storage)
        {
            this.storage = storage;
        }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public int Database
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the multiplexer.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ConnectionMultiplexer Multiplexer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The create batch.
        /// </summary>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="IBatch"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IBatch CreateBatch(object asyncState = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The create transaction.
        /// </summary>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="ITransaction"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ITransaction CreateTransaction(object asyncState = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The debug object.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue DebugObject(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The debug object async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash decrement.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long HashDecrement(
            RedisKey key,
            RedisValue hashField,
            long value = 1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash decrement.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double HashDecrement(
            RedisKey key,
            RedisValue hashField,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash decrement async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> HashDecrementAsync(
            RedisKey key,
            RedisValue hashField,
            long value = 1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash decrement async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double> HashDecrementAsync(
            RedisKey key,
            RedisValue hashField,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash delete.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            lock (this.storage)
            {
                object val;
                return this.storage.TryRemove(key, out val);
            }
        }

        /// <summary>
        /// The hash delete.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashFields">
        /// The hash fields.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash delete async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash delete async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashFields">
        /// The hash fields.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash exists async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash get.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash get.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashFields">
        /// The hash fields.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash get all.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="HashEntry[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash get all async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash get async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash get async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashFields">
        /// The hash fields.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> HashGetAsync(
            RedisKey key,
            RedisValue[] hashFields,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash increment.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long HashIncrement(
            RedisKey key,
            RedisValue hashField,
            long value = 1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash increment.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double HashIncrement(
            RedisKey key,
            RedisValue hashField,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash increment async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> HashIncrementAsync(
            RedisKey key,
            RedisValue hashField,
            long value = 1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash increment async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double> HashIncrementAsync(
            RedisKey key,
            RedisValue hashField,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash keys.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash keys async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> HashKeysAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash length.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash length async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash scan.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash scan.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="cursor">
        /// The cursor.
        /// </param>
        /// <param name="pageOffset">
        /// The page offset.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<HashEntry> HashScan(
            RedisKey key,
            RedisValue pattern = new RedisValue(),
            int pageSize = 10,
            long cursor = 0,
            int pageOffset = 0,
            CommandFlags flags = CommandFlags.None)
        {
            lock (this.storage)
            {
                object val;
                if (this.storage.TryGetValue(key, out val))
                {
                    var hash = val as Dictionary<string, string>;
                    if (!(val is Dictionary<string, string>))
                    {
                        throw new InvalidOperationException("Theris already other data and it is not a hash");
                    }

                    return hash.Select(p => new HashEntry(p.Key, p.Value));
                }

                return new HashEntry[0];
            }
        }

        /// <summary>
        /// The hash set.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashFields">
        /// The hash fields.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            lock (this.storage)
            {
                object val;
                var hash = new Dictionary<string, string>();
                if (this.storage.TryGetValue(key, out val))
                {
                    hash = val as Dictionary<string, string>;
                    if (hash == null)
                    {
                        throw new InvalidOperationException("Theris already other data and it is not a hash");
                    }
                }

                foreach (var hashEntry in hashFields)
                {
                    hash[hashEntry.Name] = hashEntry.Value;
                }

                this.storage[key] = hash;
            }
        }

        /// <summary>
        /// The hash set.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool HashSet(
            RedisKey key,
            RedisValue hashField,
            RedisValue value,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash set async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashFields">
        /// The hash fields.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash set async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="hashField">
        /// The hash field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> HashSetAsync(
            RedisKey key,
            RedisValue hashField,
            RedisValue value,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash values.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash values async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log add.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool HyperLogLogAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log add.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool HyperLogLogAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log add async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log add async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log length.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long HyperLogLogLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log length.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long HyperLogLogLength(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log length async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> HyperLogLogLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log length async.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> HyperLogLogLengthAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log merge.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void HyperLogLogMerge(
            RedisKey destination,
            RedisKey first,
            RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log merge.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="sourceKeys">
        /// The source keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void HyperLogLogMerge(
            RedisKey destination,
            RedisKey[] sourceKeys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log merge async.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task HyperLogLogMergeAsync(
            RedisKey destination,
            RedisKey first,
            RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hyper log log merge async.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="sourceKeys">
        /// The source keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task HyperLogLogMergeAsync(
            RedisKey destination,
            RedisKey[] sourceKeys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The identify endpoint.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="EndPoint"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public EndPoint IdentifyEndpoint(RedisKey key = new RedisKey(), CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The identify endpoint async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<EndPoint> IdentifyEndpointAsync(
            RedisKey key = new RedisKey(),
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The is connected.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool IsConnected(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key delete.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            lock (this.storage)
            {
                object val;
                return this.storage.TryRemove(key, out val);
            }
        }

        /// <summary>
        /// The key delete.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key delete async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key delete async.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key dump.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public byte[] KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key dump async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<byte[]> KeyDumpAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key exists async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key expire.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key expire.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key expire async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key expire async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key migrate.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="toServer">
        /// The to server.
        /// </param>
        /// <param name="toDatabase">
        /// The to database.
        /// </param>
        /// <param name="timeoutMilliseconds">
        /// The timeout milliseconds.
        /// </param>
        /// <param name="migrateOptions">
        /// The migrate options.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void KeyMigrate(
            RedisKey key,
            EndPoint toServer,
            int toDatabase = 0,
            int timeoutMilliseconds = 0,
            MigrateOptions migrateOptions = MigrateOptions.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key migrate async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="toServer">
        /// The to server.
        /// </param>
        /// <param name="toDatabase">
        /// The to database.
        /// </param>
        /// <param name="timeoutMilliseconds">
        /// The timeout milliseconds.
        /// </param>
        /// <param name="migrateOptions">
        /// The migrate options.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task KeyMigrateAsync(
            RedisKey key,
            EndPoint toServer,
            int toDatabase = 0,
            int timeoutMilliseconds = 0,
            MigrateOptions migrateOptions = MigrateOptions.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key move.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key move async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyMoveAsync(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key persist.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key persist async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyPersistAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key random.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisKey"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisKey KeyRandom(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key random async.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisKey> KeyRandomAsync(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key rename.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="newKey">
        /// The new key.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool KeyRename(
            RedisKey key,
            RedisKey newKey,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key rename async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="newKey">
        /// The new key.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> KeyRenameAsync(
            RedisKey key,
            RedisKey newKey,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key restore.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void KeyRestore(
            RedisKey key,
            byte[] value,
            TimeSpan? expiry = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key restore async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task KeyRestoreAsync(
            RedisKey key,
            byte[] value,
            TimeSpan? expiry = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key time to live.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="TimeSpan?"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key time to live async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key type.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisType"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisType KeyType(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The key type async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisType> KeyTypeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list get by index.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list get by index async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> ListGetByIndexAsync(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list insert after.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pivot">
        /// The pivot.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListInsertAfter(
            RedisKey key,
            RedisValue pivot,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list insert after async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pivot">
        /// The pivot.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListInsertAfterAsync(
            RedisKey key,
            RedisValue pivot,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list insert before.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pivot">
        /// The pivot.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListInsertBefore(
            RedisKey key,
            RedisValue pivot,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list insert before async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pivot">
        /// The pivot.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListInsertBeforeAsync(
            RedisKey key,
            RedisValue pivot,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list left pop.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list left pop async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> ListLeftPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list left push.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListLeftPush(
            RedisKey key,
            RedisValue value,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list left push.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list left push async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListLeftPushAsync(
            RedisKey key,
            RedisValue value,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list left push async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list length.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list length async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list range.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] ListRange(
            RedisKey key,
            long start = 0,
            long stop = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list range async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> ListRangeAsync(
            RedisKey key,
            long start = 0,
            long stop = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list remove async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListRemoveAsync(
            RedisKey key,
            RedisValue value,
            long count = 0,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right pop.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right pop async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> ListRightPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right pop left push.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue ListRightPopLeftPush(
            RedisKey source,
            RedisKey destination,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right pop left push async.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> ListRightPopLeftPushAsync(
            RedisKey source,
            RedisKey destination,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right push.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListRightPush(
            RedisKey key,
            RedisValue value,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right push.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right push async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListRightPushAsync(
            RedisKey key,
            RedisValue value,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list right push async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list set by index.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list set by index async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task ListSetByIndexAsync(
            RedisKey key,
            long index,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list trim.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The list trim async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task ListTrimAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock extend.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock extend async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> LockExtendAsync(
            RedisKey key,
            RedisValue value,
            TimeSpan expiry,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock query.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock query async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> LockQueryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock release.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock release async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock take.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The lock take async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> LockTakeAsync(
            RedisKey key,
            RedisValue value,
            TimeSpan expiry,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The ping.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="TimeSpan"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The ping async.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The publish.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The publish async.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate.
        /// </summary>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisResult"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisResult ScriptEvaluate(
            string script,
            RedisKey[] keys = null,
            RedisValue[] values = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisResult"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisResult ScriptEvaluate(
            byte[] hash,
            RedisKey[] keys = null,
            RedisValue[] values = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate.
        /// </summary>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisResult"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisResult ScriptEvaluate(
            LuaScript script,
            object parameters = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate.
        /// </summary>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisResult"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisResult ScriptEvaluate(
            LoadedLuaScript script,
            object parameters = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate async.
        /// </summary>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisResult> ScriptEvaluateAsync(
            string script,
            RedisKey[] keys = null,
            RedisValue[] values = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate async.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisResult> ScriptEvaluateAsync(
            byte[] hash,
            RedisKey[] keys = null,
            RedisValue[] values = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate async.
        /// </summary>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisResult> ScriptEvaluateAsync(
            LuaScript script,
            object parameters = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The script evaluate async.
        /// </summary>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisResult> ScriptEvaluateAsync(
            LoadedLuaScript script,
            object parameters = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set add.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set add.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set add async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set add async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SetCombine(
            SetOperation operation,
            RedisKey first,
            RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine and store.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SetCombineAndStore(
            SetOperation operation,
            RedisKey destination,
            RedisKey first,
            RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine and store.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SetCombineAndStore(
            SetOperation operation,
            RedisKey destination,
            RedisKey[] keys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine and store async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SetCombineAndStoreAsync(
            SetOperation operation,
            RedisKey destination,
            RedisKey first,
            RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine and store async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SetCombineAndStoreAsync(
            SetOperation operation,
            RedisKey destination,
            RedisKey[] keys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SetCombineAsync(
            SetOperation operation,
            RedisKey first,
            RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set combine async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SetCombineAsync(
            SetOperation operation,
            RedisKey[] keys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set contains.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set contains async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set length.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set length async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set members.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set members async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SetMembersAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set move.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SetMove(
            RedisKey source,
            RedisKey destination,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set move async.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> SetMoveAsync(
            RedisKey source,
            RedisKey destination,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set pop.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set pop async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> SetPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set random member.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set random member async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> SetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set random members.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set random members async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SetRandomMembersAsync(
            RedisKey key,
            long count,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set remove async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> SetRemoveAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set remove async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SetRemoveAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set scan.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set scan.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="cursor">
        /// The cursor.
        /// </param>
        /// <param name="pageOffset">
        /// The page offset.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<RedisValue> SetScan(
            RedisKey key,
            RedisValue pattern = new RedisValue(),
            int pageSize = 10,
            long cursor = 0,
            int pageOffset = 0,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sort.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="sortType">
        /// The sort type.
        /// </param>
        /// <param name="by">
        /// The by.
        /// </param>
        /// <param name="get">
        /// The get.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] Sort(
            RedisKey key,
            long skip = 0,
            long take = -1,
            Order order = Order.Ascending,
            SortType sortType = SortType.Numeric,
            RedisValue @by = new RedisValue(),
            RedisValue[] get = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sort and store.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="sortType">
        /// The sort type.
        /// </param>
        /// <param name="by">
        /// The by.
        /// </param>
        /// <param name="get">
        /// The get.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortAndStore(
            RedisKey destination,
            RedisKey key,
            long skip = 0,
            long take = -1,
            Order order = Order.Ascending,
            SortType sortType = SortType.Numeric,
            RedisValue @by = new RedisValue(),
            RedisValue[] get = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sort and store async.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="sortType">
        /// The sort type.
        /// </param>
        /// <param name="by">
        /// The by.
        /// </param>
        /// <param name="get">
        /// The get.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortAndStoreAsync(
            RedisKey destination,
            RedisKey key,
            long skip = 0,
            long take = -1,
            Order order = Order.Ascending,
            SortType sortType = SortType.Numeric,
            RedisValue @by = new RedisValue(),
            RedisValue[] get = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sort async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="sortType">
        /// The sort type.
        /// </param>
        /// <param name="by">
        /// The by.
        /// </param>
        /// <param name="get">
        /// The get.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SortAsync(
            RedisKey key,
            long skip = 0,
            long take = -1,
            Order order = Order.Ascending,
            SortType sortType = SortType.Numeric,
            RedisValue @by = new RedisValue(),
            RedisValue[] get = null,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set add.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="score">
        /// The score.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set add.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set add async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="score">
        /// The score.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> SortedSetAddAsync(
            RedisKey key,
            RedisValue member,
            double score,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set add async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetAddAsync(
            RedisKey key,
            SortedSetEntry[] values,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set combine and store.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="aggregate">
        /// The aggregate.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetCombineAndStore(
            SetOperation operation,
            RedisKey destination,
            RedisKey first,
            RedisKey second,
            Aggregate aggregate = Aggregate.Sum,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set combine and store.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="weights">
        /// The weights.
        /// </param>
        /// <param name="aggregate">
        /// The aggregate.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetCombineAndStore(
            SetOperation operation,
            RedisKey destination,
            RedisKey[] keys,
            double[] weights = null,
            Aggregate aggregate = Aggregate.Sum,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set combine and store async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="aggregate">
        /// The aggregate.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetCombineAndStoreAsync(
            SetOperation operation,
            RedisKey destination,
            RedisKey first,
            RedisKey second,
            Aggregate aggregate = Aggregate.Sum,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set combine and store async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="weights">
        /// The weights.
        /// </param>
        /// <param name="aggregate">
        /// The aggregate.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetCombineAndStoreAsync(
            SetOperation operation,
            RedisKey destination,
            RedisKey[] keys,
            double[] weights = null,
            Aggregate aggregate = Aggregate.Sum,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set decrement.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double SortedSetDecrement(
            RedisKey key,
            RedisValue member,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set decrement async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double> SortedSetDecrementAsync(
            RedisKey key,
            RedisValue member,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set increment.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double SortedSetIncrement(
            RedisKey key,
            RedisValue member,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set increment async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double> SortedSetIncrementAsync(
            RedisKey key,
            RedisValue member,
            double value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set length.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetLength(
            RedisKey key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set length async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetLengthAsync(
            RedisKey key,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set length by value.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetLengthByValue(
            RedisKey key,
            RedisValue min,
            RedisValue max,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set length by value async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetLengthByValueAsync(
            RedisKey key,
            RedisValue min,
            RedisValue max,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by rank.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SortedSetRangeByRank(
            RedisKey key,
            long start = 0,
            long stop = -1,
            Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by rank async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SortedSetRangeByRankAsync(
            RedisKey key,
            long start = 0,
            long stop = -1,
            Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by rank with scores.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="SortedSetEntry[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public SortedSetEntry[] SortedSetRangeByRankWithScores(
            RedisKey key,
            long start = 0,
            long stop = -1,
            Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by rank with scores async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(
            RedisKey key,
            long start = 0,
            long stop = -1,
            Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by score.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SortedSetRangeByScore(
            RedisKey key,
            double start = double.NegativeInfinity,
            double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None,
            Order order = Order.Ascending,
            long skip = 0,
            long take = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by score async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SortedSetRangeByScoreAsync(
            RedisKey key,
            double start = double.NegativeInfinity,
            double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None,
            Order order = Order.Ascending,
            long skip = 0,
            long take = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by score with scores.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="SortedSetEntry[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public SortedSetEntry[] SortedSetRangeByScoreWithScores(
            RedisKey key,
            double start = double.NegativeInfinity,
            double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None,
            Order order = Order.Ascending,
            long skip = 0,
            long take = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by score with scores async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(
            RedisKey key,
            double start = double.NegativeInfinity,
            double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None,
            Order order = Order.Ascending,
            long skip = 0,
            long take = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by value.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] SortedSetRangeByValue(
            RedisKey key,
            RedisValue min = new RedisValue(),
            RedisValue max = new RedisValue(),
            Exclude exclude = Exclude.None,
            long skip = 0,
            long take = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set range by value async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="skip">
        /// The skip.
        /// </param>
        /// <param name="take">
        /// The take.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> SortedSetRangeByValueAsync(
            RedisKey key,
            RedisValue min = new RedisValue(),
            RedisValue max = new RedisValue(),
            Exclude exclude = Exclude.None,
            long skip = 0,
            long take = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set rank.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long?"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long? SortedSetRank(
            RedisKey key,
            RedisValue member,
            Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set rank async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long?> SortedSetRankAsync(
            RedisKey key,
            RedisValue member,
            Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="members">
        /// The members.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="members">
        /// The members.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetRemoveAsync(
            RedisKey key,
            RedisValue[] members,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove range by rank.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetRemoveRangeByRank(
            RedisKey key,
            long start,
            long stop,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove range by rank async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetRemoveRangeByRankAsync(
            RedisKey key,
            long start,
            long stop,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove range by score.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetRemoveRangeByScore(
            RedisKey key,
            double start,
            double stop,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove range by score async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetRemoveRangeByScoreAsync(
            RedisKey key,
            double start,
            double stop,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove range by value.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long SortedSetRemoveRangeByValue(
            RedisKey key,
            RedisValue min,
            RedisValue max,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set remove range by value async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> SortedSetRemoveRangeByValueAsync(
            RedisKey key,
            RedisValue min,
            RedisValue max,
            Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set scan.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<SortedSetEntry> SortedSetScan(
            RedisKey key,
            RedisValue pattern,
            int pageSize,
            CommandFlags flags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set scan.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="cursor">
        /// The cursor.
        /// </param>
        /// <param name="pageOffset">
        /// The page offset.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<SortedSetEntry> SortedSetScan(
            RedisKey key,
            RedisValue pattern = new RedisValue(),
            int pageSize = 10,
            long cursor = 0,
            int pageOffset = 0,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set score.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double?"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The sorted set score async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double?> SortedSetScoreAsync(
            RedisKey key,
            RedisValue member,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string append.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string append async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringAppendAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit count.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit count async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringBitCountAsync(
            RedisKey key,
            long start = 0,
            long end = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit operation.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringBitOperation(
            Bitwise operation,
            RedisKey destination,
            RedisKey first,
            RedisKey second = new RedisKey(),
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit operation.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringBitOperation(
            Bitwise operation,
            RedisKey destination,
            RedisKey[] keys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit operation async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <param name="second">
        /// The second.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringBitOperationAsync(
            Bitwise operation,
            RedisKey destination,
            RedisKey first,
            RedisKey second = new RedisKey(),
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit operation async.
        /// </summary>
        /// <param name="operation">
        /// The operation.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringBitOperationAsync(
            Bitwise operation,
            RedisKey destination,
            RedisKey[] keys,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit position.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="bit">
        /// The bit.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringBitPosition(
            RedisKey key,
            bool bit,
            long start = 0,
            long end = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string bit position async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="bit">
        /// The bit.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringBitPositionAsync(
            RedisKey key,
            bool bit,
            long start = 0,
            long end = -1,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string decrement.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string decrement.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string decrement async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string decrement async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get async.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get bit.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get bit async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> StringGetBitAsync(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get range.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get range async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="end">
        /// The end.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> StringGetRangeAsync(
            RedisKey key,
            long start,
            long end,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get set.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            lock (this.storage)
            {
                object result;
                if (!this.storage.TryGetValue(key, out result))
                {
                    result = null;
                }

                this.storage[key] = (string)value;

                return (string)result;
            }
        }

        /// <summary>
        /// The string get set async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> StringGetSetAsync(
            RedisKey key,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get with expiry.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValueWithExpiry"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string get with expiry async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValueWithExpiry> StringGetWithExpiryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string increment.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string increment.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string increment async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string increment async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string length.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string length async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> StringLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool StringSet(
            RedisKey key,
            RedisValue value,
            TimeSpan? expiry = null,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            lock (this.storage)
            {
                string stringKey = key;
                switch (when)
                {
                    case When.Always:
                        this.storage[stringKey] = (string)value;
                        return true;

                    case When.Exists:
                        if (this.storage.ContainsKey(stringKey))
                        {
                            this.storage[stringKey] = (string)value;
                            return true;
                        }
                        return false;

                    case When.NotExists:
                        if (!this.storage.ContainsKey(stringKey))
                        {
                            this.storage[stringKey] = (string)value;
                            return true;
                        }
                        return false;

                    default:
                        throw new ArgumentException(nameof(when));
                }
            }
        }

        /// <summary>
        /// The string set.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool StringSet(
            KeyValuePair<RedisKey, RedisValue>[] values,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> StringSetAsync(
            RedisKey key,
            RedisValue value,
            TimeSpan? expiry = null,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set async.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="when">
        /// The when.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> StringSetAsync(
            KeyValuePair<RedisKey, RedisValue>[] values,
            When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set bit.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="bit">
        /// The bit.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool StringSetBit(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set bit async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="bit">
        /// The bit.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> StringSetBitAsync(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set range.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="RedisValue"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public RedisValue StringSetRange(
            RedisKey key,
            long offset,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The string set range async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<RedisValue> StringSetRangeAsync(
            RedisKey key,
            long offset,
            RedisValue value,
            CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The try wait.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool TryWait(Task task)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The wait.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Wait(Task task)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The wait.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public T Wait<T>(Task<T> task)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The wait all.
        /// </summary>
        /// <param name="tasks">
        /// The tasks.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void WaitAll(params Task[] tasks)
        {
            throw new NotImplementedException();
        }
    }
}