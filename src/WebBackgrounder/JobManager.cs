using System;
using System.Collections.Generic;
using System.Threading;

namespace WebBackgrounder
{
    public class JobManager : IDisposable
    {
        readonly IJobHost _host;
        readonly Timer _timer;
        readonly IJobCoordinator _coordinator;
        readonly Scheduler _scheduler;
        Action<Exception> _failHandler;

        public JobManager(IEnumerable<IJob> jobs, IJobHost host) : this(jobs, host, new SingleServerJobCoordinator())
        {
        }

        public JobManager(IEnumerable<IJob> jobs, IJobCoordinator coordinator) : this(jobs, new AspNetTaskHost(), coordinator)
        {
        }

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
            _timer.Stop();

            try
            {
                using (var schedule = _scheduler.Next())
                {
                    RunJob(schedule.Job);
                }
            }
            catch (Exception e)
            {
                OnException(e); // Someone else's problem now. :)
            }
            finally
            {
                _timer.Next(_scheduler.Next().Job.Interval); // Start up again.
            }
        }

        public void RunJob(IJob job)
        {
            lock (_host)
            {
                if (_host.ShuttingDown)
                {
                    return;
                }
                _coordinator.PerformWork(job);
            }
        }

        public void Dispose()
        {
            Stop();
            _timer.Dispose();
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

