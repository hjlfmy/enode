﻿using System;
using System.Threading;

namespace ENode.Infrastructure
{
    /// <summary>Represent a background worker that will repeatedly execute a specific method.
    /// </summary>
    public class Worker
    {
        private bool _stopped;
        private readonly Action _action;
        private readonly Thread _thread;
        private readonly ILogger _logger;

        /// <summary>Return the IsAlive status of the current worker.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return _thread.IsAlive;
            }
        }

        /// <summary>Initialize a new Worker for the specified method to run.
        /// </summary>
        /// <param name="action">The delegate method to execute in a loop.</param>
        public Worker(Action action)
        {
            _action = action;
            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.Name = string.Format("Worker thread {0}", _thread.ManagedThreadId);
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(_thread.Name);
        }

        /// <summary>Start the worker.
        /// </summary>
        public Worker Start()
        {
            if (!_thread.IsAlive)
            {
                _thread.Start();
            }
            return this;
        }
        /// <summary>Stop the worker.
        /// </summary>
        public Worker Stop()
        {
            _stopped = true;
            return this;
        }

        /// <summary>Executes the delegate method until the <see cref="Stop"/> method is called.
        /// </summary>
        private void Loop()
        {
            while (!_stopped)
            {
                try
                {
                    _action();
                }
                catch (ThreadAbortException abortException)
                {
                    _logger.Error("caught ThreadAbortException - resetting.", abortException);
                    Thread.ResetAbort();
                    _logger.Info("ThreadAbortException resetted.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Exception raised when executing worker delegate.", ex);
                }
            }
        }
    }
}
