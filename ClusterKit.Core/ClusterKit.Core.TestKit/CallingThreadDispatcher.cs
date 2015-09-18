// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallingThreadDispatcher.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The calling thread dispatcher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using Akka.Actor;
    using Akka.Dispatch;

    /// <summary>
    /// The calling thread dispatcher for tests.
    /// </summary>
    public class CallingThreadDispatcher : MessageDispatcher
    {
        /// <summary>
        /// EventWaitHandle to wait for empty queue
        /// </summary>
        private static readonly EventWaitHandle WorkDoneHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        /// <summary>
        /// Total amount of work queue
        /// </summary>
        private static int runningTasks;

        /// <summary>
        /// Working thread number
        /// </summary>
        private static int threadNum;

        /// <summary>
        /// Work queue
        /// </summary>
        private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// Threads blocker in work queue cycle for current task
        /// </summary>
        private readonly EventWaitHandle currentWorkDone = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// Current dispatcher number
        /// </summary>
        private readonly int num;

        /// <summary>
        /// Threads blocker in work queue cycle for queue
        /// </summary>
        private readonly EventWaitHandle queueHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// Task executing thread
        /// </summary>
        private readonly Thread workingThread;

        /// <summary>
        /// Current actor path (just for display purposes)
        /// </summary>
        private string cellPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallingThreadDispatcher"/> class.
        /// </summary>
        /// <param name="configurator">
        /// The configurator.
        /// </param>
        public CallingThreadDispatcher(MessageDispatcherConfigurator configurator)
            : base(configurator)
        {
            this.num = Interlocked.Add(ref threadNum, 1);
            this.workingThread = new Thread(this.DoWork);
            this.workingThread.Start();
        }

        /// <summary>
        /// Gets or sets a value indicating whether concurrent mode between dispatchers is enabled.
        /// </summary>
        public static bool ConcurrentMode { get; set; }

        /// <summary>
        /// Gets the current dispatcher name.
        /// </summary>
        public string Name
        {
            get
            {
                return string.Format("CallingThreadDispatcher-{0}-{1}", this.num, this.cellPath);
            }
        }

        /// <summary>
        ///  Rise block to wait work done (at least one task should be done)
        /// </summary>
        public static void RiseBlock()
        {
            WorkDoneHandle.Reset();
        }

        /// <summary>
        /// Blocks current thread till all work is done
        /// </summary>
        public static void WaitForAllDone()
        {
            WorkDoneHandle.WaitOne();
        }

        /// <summary>
        /// Blocks current thread till all work is done
        /// </summary>
        /// <param name="timeout">
        /// Maximum wait time
        /// </param>
        /// <returns>
        /// This method will return either when all work is done (true) or timeout have elapsed (false).
        /// </returns>
        public static bool WaitForAllDone(TimeSpan timeout)
        {
            return WorkDoneHandle.WaitOne(timeout);
        }

        /// <summary>
        /// Attaches the dispatcher to the <see cref="T:Akka.Actor.ActorCell"/>
        /// <remarks>
        /// Practically, doesn't do very much right now - dispatchers aren't responsible for creating
        ///             mailboxes in Akka.NET
        /// </remarks>
        /// </summary>
        /// <param name="cell">The ActorCell belonging to the actor who's attaching to this dispatcher.</param>
        public override void Attach(ActorCell cell)
        {
            base.Attach(cell);
            this.cellPath = cell.Self.Path.ToString();
            this.workingThread.Name = this.Name;
        }

        /// <summary>
        /// Detaches the dispatcher to the <see cref="T:Akka.Actor.ActorCell"/>
        /// <remarks>
        /// Only really used in dispatchers with 1:1 relationship with dispatcher.
        /// </remarks>
        /// </summary>
        /// <param name="cell">The ActorCell belonging to the actor who's detaching from this dispatcher.</param>
        public override void Detach(ActorCell cell)
        {
            base.Detach(cell);
            this.workingThread.Abort();
        }

        /// <summary>
        /// Schedules the specified run.
        /// </summary>
        /// <param name="run">The run.</param>
        public override void Schedule(Action run)
        {
            if (Thread.CurrentThread.Name == this.Name)
            {
                // in case of recursive load, we will just run task in current thread
                run();
            }
            else
            {
                Interlocked.Add(ref runningTasks, 1);
                WorkDoneHandle.Reset();

                // run();
                this.currentWorkDone.Reset();
                this.actionQueue.Enqueue(run);
                this.queueHandle.Set();

                if (!ConcurrentMode)
                {
                    this.currentWorkDone.WaitOne();
                }
            }
        }

        /// <summary>
        /// Endless cycle of waiting and doing some work
        /// </summary>
        private void DoWork()
        {
            try
            {
                while (true)
                {
                    Action action;
                    while (this.actionQueue.TryDequeue(out action))
                    {
                        try
                        {
                            action();
                        }
                        finally
                        {
                            var workLeft = Interlocked.Add(ref runningTasks, -1);
                            if (workLeft == 0)
                            {
                                WorkDoneHandle.Set();
                            }

                            this.currentWorkDone.Set();
                        }
                    }

                    this.queueHandle.WaitOne();
                }
            }
            catch (ThreadAbortException)
            {
            }
        }
    }
}