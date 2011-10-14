using System;
using System.Collections.Generic;
using System.Linq;

namespace WebBackgrounder
{
    public class Scheduler
    {
        readonly IEnumerable<Schedule> _schedules;

        public Scheduler(IEnumerable<IJob> jobs, Func<DateTime> nowThunk)
        {
            if (jobs.Any(j => j.Interval < TimeSpan.Zero))
            {
                throw new ArgumentException("A job cannot have a negative interval.", "jobs");
            }
            var lastRunTime = nowThunk();
            _schedules = jobs.Select(job => new Schedule(job, nowThunk) { LastRunTime = lastRunTime }).ToList();
        }

        public Scheduler(IEnumerable<IJob> jobs) : this(jobs, () => DateTime.UtcNow)
        {
        }

        public Schedule Next()
        {
            var schedules = _schedules.OrderBy(s => s.NextRunTime);
            return schedules.First();
        }
    }
}
