using System;

namespace WebBackgrounder
{
    public class Schedule : IDisposable
    {
        private DateTime? _lastRunTime;

        public Schedule(IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }
            Job = job;
        }

        public IJob Job { get; private set; }

        public DateTime? LastRunTime
        {
            get { return _lastRunTime ?? (_lastRunTime = DateTime.UtcNow); }
            set { _lastRunTime = value ?? DateTime.UtcNow; }
        }

        public DateTime NextRunTime
        {
            get
            {
                var lastRunTime = LastRunTime ?? DateTime.UtcNow;
                return lastRunTime.Add(Job.Interval);
            }
        }

        public TimeSpan GetIntervalToNextRun()
        {
            var now = DateTime.UtcNow;
            if (NextRunTime < now)
            {
                return TimeSpan.FromMilliseconds(1);
            }
            return NextRunTime - now;
        }

        void IDisposable.Dispose()
        {
            LastRunTime = DateTime.UtcNow;
        }
    }
}
