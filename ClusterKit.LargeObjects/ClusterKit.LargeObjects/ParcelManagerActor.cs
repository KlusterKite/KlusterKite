// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelManagerActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Manager that handles sending parcels
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;
    using Akka.Util.Internal;

    using Autofac;

    using ClusterKit.Core.Utils;
    using ClusterKit.LargeObjects.Client;

    using JetBrains.Annotations;

    using Serilog;

    /// <summary>
    /// Manager that handles sending parcels
    /// </summary>
    /// <remarks>
    /// Should be singleton in local system.
    /// In case of system failure all parcells will be lost.
    /// Try to avoid using this, as this is really slow and large messages typically means some errors in software design.
    /// </remarks>
    [UsedImplicitly]
    public class ParcelManagerActor : ReceiveActor
    {
        /// <summary>
        /// The global parcel storage
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, Parcel> Parcels = new ConcurrentDictionary<Guid, Parcel>();

        /// <summary>
        /// The list of parcel notification envelopers
        /// </summary>
        private readonly List<INotificationEnveloper> envelopers;

        /// <summary>
        /// The tcp stream read timeout
        /// </summary>
        private readonly TimeSpan readTimeout;

        /// <summary>
        /// The local server host
        /// </summary>
        private string host;

        /// <summary>
        /// The tcp listener service
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// Current listener task
        /// </summary>
        private Task listenerTask;

        /// <summary>
        /// The local server port
        /// </summary>
        private int port;

        /// <summary>
        /// The actor system
        /// </summary>
        private ActorSystem sys;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelManagerActor"/> class.
        /// </summary>
        /// <param name="container">
        /// The DI container.
        /// </param>
        public ParcelManagerActor(IComponentContext container)
        {
            this.readTimeout = Context.System.Settings.Config.GetTimeSpan(
                "ClusterKit.LargeObjects.TcpReadTimeout",
                TimeSpan.FromSeconds(10));
            this.envelopers =
                container.Resolve<IEnumerable<INotificationEnveloper>>()
                    .OrderByDescending(e => e.Priority)
                    .ToList();
            this.Receive<Parcel>(m => this.OnSetLargeObjectMessage(m));
            this.Receive<CleanUpMessage>(m => this.CleanUp());
        }

        /// <summary>
        ///     User overridable callback.
        ///     <p />
        ///     Is called asynchronously after 'actor.stop()' is invoked.
        ///     Empty default implementation.
        /// </summary>
        protected override void PostStop()
        {
            this.listener?.Stop();
            this.listener = null;
            this.listenerTask.Wait();
            base.PostStop();
        }

        /// <summary>
        ///     User overridable callback: '''By default it disposes of all children and then calls `postStop()`.'''
        ///     <p />
        ///     Is called on a crashed Actor right BEFORE it is restarted to allow clean
        ///     up of resources before Actor is terminated.
        /// </summary>
        /// <param name="reason">the Exception that caused the restart to happen.</param>
        /// <param name="message">optionally the current message the actor processed when failing, if applicable.</param>
        protected override void PreRestart(Exception reason, object message)
        {
            this.listener?.Stop();
            this.listener = null;
            this.listenerTask.Wait();
            base.PreRestart(reason, message);
        }

        /// <summary>
        ///     User overridable callback.
        ///     <p />
        ///     Is called when an Actor is started.
        ///     Actors are automatically started asynchronously when created.
        ///     Empty default implementation.
        /// </summary>
        protected override void PreStart()
        {
            base.PreStart();
            this.listener = new TcpListener(IPAddress.Any, 0);
            this.listener.Start();
            this.port = ((IPEndPoint)this.listener.LocalEndpoint).Port;
            this.sys = Context.System;

            var config = Context.System.Settings.Config;
            this.host = config.GetString(
                "akka.remote.helios.tcp.public-hostname",
                config.GetString("akka.remote.helios.tcp.hostname", "127.0.0.1"));

#pragma warning disable 4014
            this.listenerTask = this.Listen();
#pragma warning restore 4014

            Context.GetLogger()
                .Info("{Type}: started parcel server on {host}:{port}", this.GetType().Name, this.host, this.port);
            Parcels.Values.ForEach(this.SendNotification);

            var cleanUpInterval = TimeSpan.FromMinutes(1);
            this.sys.Scheduler.ScheduleTellRepeatedly(
                cleanUpInterval,
                cleanUpInterval,
                this.Self,
                new CleanUpMessage(),
                this.Self);
        }

        /// <summary>
        /// Remove obsolete messages
        /// </summary>
        private void CleanUp()
        {
            var now = this.sys.Scheduler.Now;
            var obsoleteMessages = Parcels.Values.Where(p => (now - p.SentTime) > p.StoreTimeout).ToList();
            foreach (var obsoleteMessage in obsoleteMessages)
            {
                Parcel parcel;
                Parcels.TryRemove(obsoleteMessage.Uid, out parcel);
            }
        }

        /// <summary>
        /// Handles the single parcel server connections
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <returns>The async task</returns>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task HandleConnection(TcpClient connection)
        {
            try
            {
                using (var stream = connection.GetStream())
                {
                    var guidBytes = new byte[16];
                    var readBytes = await this.ReadWithTimeout(stream, guidBytes, 0, 16, this.readTimeout);
                    if (readBytes != 16)
                    {
                        await stream.WriteAsync(new[] { (byte)EnParcelServerResponseCode.BadRequest }, 0, 1);
                        return;
                    }

                    Guid uid;
                    try
                    {
                        uid = new Guid(guidBytes);
                    }
                    catch
                    {
                        await stream.WriteAsync(new[] { (byte)EnParcelServerResponseCode.BadRequest }, 0, 1);
                        return;
                    }

                    Parcel parcel;
                    if (!Parcels.TryRemove(uid, out parcel))
                    {
                        await stream.WriteAsync(new[] { (byte)EnParcelServerResponseCode.NotFound }, 0, 1);
                        return;
                    }

                    await stream.WriteAsync(new[] { (byte)EnParcelServerResponseCode.Ok }, 0, 1);
                    var data = parcel.Payload.SerializeToAkka(this.sys);
                    await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "{Type}: error while handling connection", this.GetType().Name);
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Processes incoming connections
        /// </summary>
        /// <returns>The async task</returns>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task Listen()
        {
            try
            {
                while (this.listener != null)
                {
                    var client = await this.listener?.AcceptTcpClientAsync();
#pragma warning disable 4014
                    if (client != null)
                    {
                        this.HandleConnection(client);
                    }

#pragma warning restore 4014
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        /// Handles the <see cref="Parcel"/> message
        /// </summary>
        /// <param name="parcel">The parcel</param>
        private void OnSetLargeObjectMessage(Parcel parcel)
        {
            parcel.Sender = this.Sender;
            Parcels[parcel.Uid] = parcel;
            this.SendNotification(parcel);
        }

        /// <summary>
        /// Reads from the stream with provided timeout
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="buffer">The byte buffer</param>
        /// <param name="offset">The stream offset</param>
        /// <param name="length">The number of bytes to read</param>
        /// <param name="timeout">The timeout</param>
        /// <returns>The number of read bytes</returns>
        private async Task<int> ReadWithTimeout(
            NetworkStream stream,
            byte[] buffer,
            int offset,
            int length,
            TimeSpan timeout)
        {
            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            var task = stream.ReadAsync(buffer, offset, length, ct);

            if (await Task.WhenAny(task, Task.Delay(timeout, ct)) == task)
            {
                tokenSource.Cancel();
                return task.Result;
            }

            tokenSource.Cancel();
            throw new TimeoutException();
        }

        /// <summary>
        /// Sends parcel notification
        /// </summary>
        /// <param name="parcel">The parcel</param>
        private void SendNotification(Parcel parcel)
        {
            var notification = new ParcelNotification
                                   {
                                       Host = this.host,
                                       Port = this.port,
                                       Uid = parcel.Uid,
                                       PayloadTypeName =
                                           parcel.Payload.GetType().AssemblyQualifiedName
                                   };

            object envelope = null;
            foreach (var notificationEnveloper in this.envelopers)
            {
                envelope = notificationEnveloper.Envelope(parcel, notification);
                if (envelope != null)
                {
                    break;
                }
            }

            parcel.Recipient.Tell(envelope ?? notification, this.Sender);
        }

        /// <summary>
        /// Self-message to clean-up obsolete parcels
        /// </summary>
        private class CleanUpMessage
        {
        }
    }
}