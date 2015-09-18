// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisConnectionMoq.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Redis connection moq
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit.Moq
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    using StackExchange.Redis;

    /// <summary>
    /// Redis connection moq
    /// </summary>
    public class RedisConnectionMoq : IConnectionMultiplexer
    {
        /// <summary>
        /// moq redis storage
        /// </summary>
        private ConcurrentDictionary<string, object> storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnectionMoq"/> class.
        /// </summary>
        /// <param name="storage">
        /// The moq storage. Provide it if you want to check stored data.
        /// </param>
        public RedisConnectionMoq(ConcurrentDictionary<string, object> storage = null)
        {
            this.storage = storage ?? new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// The configuration changed.
        /// </summary>
        public event EventHandler<EndPointEventArgs> ConfigurationChanged;

        /// <summary>
        /// The configuration changed broadcast.
        /// </summary>
        public event EventHandler<EndPointEventArgs> ConfigurationChangedBroadcast;

        /// <summary>
        /// The connection failed.
        /// </summary>
        public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed;

        /// <summary>
        /// The connection restored.
        /// </summary>
        public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored;

        /// <summary>
        /// The error message.
        /// </summary>
        public event EventHandler<RedisErrorEventArgs> ErrorMessage;

        /// <summary>
        /// The hash slot moved.
        /// </summary>
        public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved;

        /// <summary>
        /// The internal error.
        /// </summary>
        public event EventHandler<InternalErrorEventArgs> InternalError;

        /// <summary>
        /// Gets the client name.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string ClientName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string Configuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether include detail in exceptions.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool IncludeDetailInExceptions
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether is connected.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the operation count.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long OperationCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether preserve async order.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool PreserveAsyncOrder
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the storm log threshold.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public int StormLogThreshold
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the timeout milliseconds.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public int TimeoutMilliseconds
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The begin profiling.
        /// </summary>
        /// <param name="forContext">
        /// The for context.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void BeginProfiling(object forContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="allowCommandsToComplete">
        /// The allow commands to complete.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Close(bool allowCommandsToComplete = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The close async.
        /// </summary>
        /// <param name="allowCommandsToComplete">
        /// The allow commands to complete.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task CloseAsync(bool allowCommandsToComplete = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The configure.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool Configure(TextWriter log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The configure async.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<bool> ConfigureAsync(TextWriter log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The finish profiling.
        /// </summary>
        /// <param name="forContext">
        /// The for context.
        /// </param>
        /// <param name="allowCleanupSweep">
        /// The allow cleanup sweep.
        /// </param>
        /// <returns>
        /// The <see cref="ProfiledCommandEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ProfiledCommandEnumerable FinishProfiling(object forContext, bool allowCleanupSweep = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get counters.
        /// </summary>
        /// <returns>
        /// The <see cref="ServerCounters"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ServerCounters GetCounters()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get database.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="IDatabase"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IDatabase GetDatabase(int db = -1, object asyncState = null)
        {
            if (db == -1)
            {
                return new RedisMoq(this.storage);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// The get end points.
        /// </summary>
        /// <param name="configuredOnly">
        /// The configured only.
        /// </param>
        /// <returns>
        /// The <see cref="EndPoint[]"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public EndPoint[] GetEndPoints(bool configuredOnly = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get server.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="IServer"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IServer GetServer(string host, int port, object asyncState = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get server.
        /// </summary>
        /// <param name="hostAndPort">
        /// The host and port.
        /// </param>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="IServer"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IServer GetServer(string hostAndPort, object asyncState = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get server.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <returns>
        /// The <see cref="IServer"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IServer GetServer(IPAddress host, int port)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get server.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="IServer"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IServer GetServer(EndPoint endpoint, object asyncState = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get status.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string GetStatus()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get status.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void GetStatus(TextWriter log)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get storm log.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string GetStormLog()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get subscriber.
        /// </summary>
        /// <param name="asyncState">
        /// The async state.
        /// </param>
        /// <returns>
        /// The <see cref="ISubscriber"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ISubscriber GetSubscriber(object asyncState = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The hash slot.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public int HashSlot(RedisKey key)
        {
            throw new NotImplementedException();
        }

        public virtual void OnConfigurationChanged(EndPointEventArgs e)
        {
            this.ConfigurationChanged?.Invoke(this, e);
        }

        public virtual void OnConfigurationChangedBroadcast(EndPointEventArgs e)
        {
            this.ConfigurationChangedBroadcast?.Invoke(this, e);
        }

        public virtual void OnConnectionFailed(ConnectionFailedEventArgs e)
        {
            this.ConnectionFailed?.Invoke(this, e);
        }

        public virtual void OnConnectionRestored(ConnectionFailedEventArgs e)
        {
            this.ConnectionRestored?.Invoke(this, e);
        }

        public virtual void OnErrorMessage(RedisErrorEventArgs e)
        {
            this.ErrorMessage?.Invoke(this, e);
        }

        public virtual void OnHashSlotMoved(HashSlotMovedEventArgs e)
        {
            this.HashSlotMoved?.Invoke(this, e);
        }

        public virtual void OnInternalError(InternalErrorEventArgs e)
        {
            this.InternalError?.Invoke(this, e);
        }

        /// <summary>
        /// The publish reconfigure.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public long PublishReconfigure(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The publish reconfigure async.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The register profiler.
        /// </summary>
        /// <param name="profiler">
        /// The profiler.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void RegisterProfiler(IProfiler profiler)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The reset storm log.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void ResetStormLog()
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