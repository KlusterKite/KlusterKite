// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeMachineScheduler.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Override of <seealso cref="SchedulerBase" /> with freezed time and jumping support.
//   For tests purposes only.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.TestKit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Akka.Actor;
    using global::Akka.Event;

    /// <summary>
    /// Override of <seealso cref="SchedulerBase"/> with freezed time and jumping support.
    /// For tests purposes only.
    /// </summary>
    public class TimeMachineScheduler : SchedulerBase
    {
        /// <summary>
        /// Lock for multithread changes
        /// </summary>
        private static readonly object TimeLock = new object();

        /// <summary>
        /// Time machine current location
        /// </summary>
        private static DateTimeOffset currentLocation = DateTimeOffset.Now;

        /// <summary>
        /// Current time machine instance
        /// </summary>
        private static TimeMachineScheduler currentMachine;

        /// <summary>
        /// The Value showing that time is passing by
        /// </summary>
        private static bool isRunning;

        /// <summary>
        /// Value showing that time should be stopped
        /// </summary>
        private static bool needStopTicking;

        /// <summary>
        /// Time quantum (minimum time step)
        /// </summary>
        private static TimeSpan precision = TimeSpan.Zero;

        /// <summary>
        /// Current time shift (difference between start time and current time machine position)
        /// </summary>
        private static TimeSpan timeOffset = TimeSpan.Zero;

        /// <summary>
        /// The logging adapter
        /// </summary>
        private readonly ILoggingAdapter log;

        /// <summary>
        /// Current scheduled tasks queue
        /// </summary>
        private readonly List<ScheduledWork> workQueue = new List<ScheduledWork>();

        /// <summary>
        /// Time machine current location
        /// </summary>
        private DateTimeOffset startLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeMachineScheduler"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public TimeMachineScheduler(ActorSystem system)
        {
            this.log = Logging.GetLogger(system, this);
            precision = system.Settings.Config.GetTimeSpan("akka.scheduler.tick-duration");
            this.startLocation = currentLocation;
            currentMachine = this;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        public override TimeSpan HighResMonotonicClock => currentLocation - this.startLocation;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        public override TimeSpan MonotonicClock => currentLocation - this.startLocation;

        /// <summary>
        /// Gets the time quantum (minimum time step).
        /// </summary>
        public static TimeSpan Precision => precision;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        protected override DateTimeOffset TimeNow => currentLocation;

        /// <summary>
        /// Gets the time machine current location
        /// </summary>
        /// <returns>The time machine current location</returns>
        public static DateTimeOffset GetCurrentLocation() => currentLocation;

        /// <summary>
        /// Gets scheduled work queue
        /// </summary>
        /// <returns>The scheduled work queue</returns>
        public static List<ScheduledWork> GetCurrentQueue()
        {
            var queue = currentMachine?.workQueue;
            if (queue == null)
            {
                return new List<ScheduledWork>();
            }

            lock (queue)
            {
                return queue.OrderBy(w => w.TimeToRun).ToList();
            }
        }

        /// <summary>
        /// Time jump
        /// </summary>
        /// <param name="jump">Size of jump</param>
        public static void Jump(TimeSpan jump)
        {
            currentLocation += jump;
            timeOffset = currentLocation - DateTimeOffset.Now;
            currentMachine?.CheckWork();
        }

        /// <summary>
        /// After using <seealso cref="JumpJustBefore"/>, jump into supposed time
        /// </summary>
        public static void JumpAfter()
        {
            Jump(Precision);
        }

        /// <summary>
        /// Jump just quantum before needed time
        /// </summary>
        /// <param name="jump">Supposed time to jump</param>
        public static void JumpJustBefore(TimeSpan jump)
        {
            Jump(jump - Precision);
        }

        /// <summary>
        /// Setting time shift acording to real server time
        /// </summary>
        /// <param name="offSet">Time shift</param>
        public static void JumpToAbsoluteOffset(TimeSpan offSet)
        {
            Jump(DateTimeOffset.Now + offSet - currentLocation);
        }

        /// <summary>
        /// Reseting current time machine location to real server time.
        /// </summary>
        public static void Reset()
        {
            currentLocation = DateTimeOffset.Now;
            if (currentMachine != null)
            {
                currentMachine.startLocation = currentLocation;
            }
        }

        /// <summary>
        /// Starting realtime flow (from current time machine location)
        /// </summary>
        public static void Start()
        {
            if (isRunning)
            {
                return;
            }

            lock (TimeLock)
            {
                if (isRunning)
                {
                    return;
                }

                needStopTicking = false;
                timeOffset = currentLocation - DateTimeOffset.Now;

                Task.Factory.StartNew(
                    () =>
                    {
                        while (true)
                        {
                            currentLocation = DateTimeOffset.Now + timeOffset;

                            currentMachine?.CheckWork();

                            if (needStopTicking)
                            {
                                isRunning = false;
                                break;
                            }

                            Thread.Sleep(precision);
                        }
                    },
                    TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// Freezing time
        /// </summary>
        public static void Stop()
        {
            if (!isRunning)
            {
                return;
            }

            lock (TimeLock)
            {
                if (!isRunning)
                {
                    return;
                }

                needStopTicking = true;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        protected override void InternalScheduleOnce(TimeSpan delay, Action action, ICancelable cancelable)
        {
            var cancellationToken = cancelable == null ? CancellationToken.None : cancelable.Token;
            this.InternalScheduleOnce(delay, action, cancellationToken);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        protected override void InternalScheduleRepeatedly(TimeSpan initialDelay, TimeSpan interval, Action action, ICancelable cancelable)
        {
            var cancellationToken = cancelable == null ? CancellationToken.None : cancelable.Token;
            this.InternalScheduleRepeatedly(initialDelay, interval, action, cancellationToken);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        protected override void InternalScheduleTellOnce(TimeSpan delay, ICanTell receiver, object message, IActorRef sender, ICancelable cancelable)
        {
            var cancellationToken = cancelable == null ? CancellationToken.None : cancelable.Token;
            this.InternalScheduleOnce(delay, () => receiver.Tell(message, sender), cancellationToken, message, receiver);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        protected override void InternalScheduleTellRepeatedly(TimeSpan initialDelay, TimeSpan interval, ICanTell receiver, object message, IActorRef sender, ICancelable cancelable)
        {
            var cancellationToken = cancelable == null ? CancellationToken.None : cancelable.Token;
            this.InternalScheduleRepeatedly(initialDelay, interval, () => receiver.Tell(message, sender), cancellationToken, message, receiver);
        }

        /// <summary>
        /// Scheduling some task
        /// </summary>
        /// <param name="delay">Timespan to wait before task execution</param>
        /// <param name="work">Task to execute</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="message">
        /// Original message to send
        /// </param>
        /// <param name="receiver">
        /// Message receiver
        /// </param>
        private void AddWork(TimeSpan delay, Action work, CancellationToken token, object message = null, ICanTell receiver = null)
        {
            if (delay <= TimeSpan.Zero)
            {
                work();
                return;
            }

            var scheduledWord = new ScheduledWork(this.TimeNow + delay, work, token, message, receiver);
            lock (this.workQueue)
            {
                this.workQueue.Add(scheduledWord);
            }
        }

        /// <summary>
        /// Checking if there is any work to do
        /// </summary>
        private void CheckWork()
        {
            List<ScheduledWork> workToDo;
            lock (this.workQueue)
            {
                workToDo = this.workQueue.Where(t => t.TimeToRun <= currentLocation).ToList();
            }

            foreach (var work in workToDo)
            {
                lock (this.workQueue)
                {
                    this.workQueue.Remove(work);
                }

                work.Action();
            }
        }

        /// <summary>
        /// Scheduling some task, after execution it would be removed
        /// </summary>
        /// <param name="initialDelay">Timespan to wait before task execution</param>
        /// <param name="action">Task to execute</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="message">
        /// Original message to send
        /// </param>
        /// <param name="receiver">
        /// Message receiver
        /// </param>
        private void InternalScheduleOnce(TimeSpan initialDelay, Action action, CancellationToken token, object message = null, ICanTell receiver = null)
        {
            Action executeAction = () =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    action();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception x)
                {
                    this.log.Error(x, "DedicatedThreadScheduler faild to execute action");
                }
            };

            this.AddWork(initialDelay, executeAction, token, message, receiver);
        }

        /// <summary>
        /// Scheduling some task, after execution it would be scheduled again
        /// </summary>
        /// <param name="initialDelay">Timespan to wait before task first execution</param>
        /// <param name="interval">Execution period</param>
        /// <param name="action">Task to execute</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="message">
        /// Original message to send
        /// </param>
        /// <param name="receiver">
        /// Message receiver
        /// </param>
        private void InternalScheduleRepeatedly(TimeSpan initialDelay, TimeSpan interval, Action action, CancellationToken token, object message = null, ICanTell receiver = null)
        {
            Action executeAction = null;
            executeAction = () =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    action();
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    this.AddWork(interval, executeAction, token, message, receiver);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception x)
                {
                    this.log.Error(x, "DedicatedThreadScheduler faild to execute action");
                }
            };

            this.AddWork(initialDelay, executeAction, token, message, receiver);
        }

        /// <summary>
        /// Scheduled task
        /// </summary>
        public class ScheduledWork
        {
            public ScheduledWork(DateTimeOffset timeToRun, Action action, CancellationToken token, object message = null, ICanTell reciever = null)
            {
                this.TimeToRun = timeToRun;
                this.Action = action;
                this.Token = token;
                this.Message = message;
                this.Reciever = reciever;
            }

            public Action Action { get; set; }

            public object Message { get; set; }

            public ICanTell Reciever { get; set; }

            public DateTimeOffset TimeToRun { get; set; }

            public CancellationToken Token { get; set; }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override string ToString()
            {
                return string.Format(
                    "{1} {0}",
                    this.TimeToRun - GetCurrentLocation(),
                    this.Message?.GetType().Name ?? "some action");
            }
        }
    }
}