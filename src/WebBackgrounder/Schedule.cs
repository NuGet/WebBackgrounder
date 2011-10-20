using System;

namespace WebBackgrounder
{
    public class Schedule : IDisposable
    {
        readonly Func<DateTime> _nowThunk;

        public Schedule(IJob job)
            : this(job, () => DateTime.UtcNow)
        {
        }

        public Schedule(IJob job, Func<DateTime> nowThunk)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }
            Job = job;
            _nowThunk = nowThunk;
        }

        public IJob Job { get; private set; }

        public DateTime LastRunTime
        {
            get;
            set;
        }

        public DateTime NextRunTime
        {
            get
            {
                return LastRunTime.Add(Job.Interval);
            }
        }

        public TimeSpan GetIntervalToNextRun()
        {
            var now = _nowThunk();
            if (NextRunTime < now)
            {
                return TimeSpan.FromMilliseconds(1);
            }
            return NextRunTime - now;
        }

        public void SetRunComplete()
        {
            LastRunTime = _nowThunk();
        }

        void IDisposable.Dispose()
        {
            SetRunComplete();
        }
    }
}
