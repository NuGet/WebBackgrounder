using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WebBackgrounder
{
    public class JobManager : IDisposable
    {
        readonly IJobHost _host;
        readonly Timer _timer;
        readonly IJobCoordinator _coordinator;
        readonly Scheduler _scheduler;
        readonly IEnumerable<IJob> _jobs;
        Action<Exception> _failHandler;

        public bool RestartSchedulerOnFailure { get; set; }

        public JobManager(IEnumerable<IJob> jobs, IJobHost host) : this(jobs, host, new SingleServerJobCoordinator()) { }

        public JobManager(IEnumerable<IJob> jobs, IJobCoordinator coordinator) : this(jobs, new JobHost(), coordinator) { }

        public JobManager(IEnumerable<IJob> jobs, IJobHost host, IJobCoordinator coordinator)
        {
            if (jobs == null)
            {
                throw new ArgumentNullException("jobs");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (coordinator == null)
            {
                throw new ArgumentNullException("coordinator");
            }

            _jobs = jobs;
            _scheduler = new Scheduler(jobs);
            _host = host;
            _coordinator = coordinator;
            _timer = new Timer(OnTimerElapsed);
        }

        public void Start()
        {
            _timer.Next(TimeSpan.FromMilliseconds(1));
        }

        public void Stop()
        {
            _timer.Stop();
        }

        void OnTimerElapsed(object sender)
        {
            try
            {
                _timer.Stop();
                DoNextJob();
                _timer.Next(_scheduler.Next().GetIntervalToNextRun()); // Start up again.
            }
            catch (Exception e)
            {
                OnException(e); // Someone else's problem.

                if (RestartSchedulerOnFailure)
                {
                    _timer.Next(_scheduler.Next().GetIntervalToNextRun()); // Start up again.
                }
            }
        }

        void DoNextJob()
        {
            using (var schedule = _scheduler.Next())
            {
                var work = _coordinator.GetWork(schedule.Job);

                if (work != null)
                {
                    _host.DoWork(work);
                }
            }
        }

        public void Dispose()
        {
            Stop();
            foreach (var job in _jobs.OfType<IDisposable>())
            {
                job.Dispose();
            }
            _timer.Dispose();
            _coordinator.Dispose();
        }

        public void Fail(Action<Exception> failHandler)
        {
            _failHandler = failHandler;
        }

        void OnException(Exception e)
        {
            var fail = _failHandler;
            if (fail != null)
            {
                fail(e);
            }
        }
    }
}