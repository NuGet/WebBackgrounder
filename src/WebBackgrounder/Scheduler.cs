using System;
using System.Collections.Generic;
using System.Linq;

namespace WebBackgrounder
{
    public class Scheduler
    {
        readonly IEnumerable<Schedule> _schedules;
        
        public Scheduler(IEnumerable<IJob> jobs)
        {
            if (jobs.Any(j => j.Interval < TimeSpan.Zero))
            {
                throw new ArgumentException("A job cannot have a negative interval.", "jobs");
            }
            _schedules = jobs.Select(job => new Schedule(job)).ToList();
        }

        public Scheduler(IEnumerable<Schedule> schedules)
        {
            _schedules = schedules;
        }

        public Schedule Next()
        {
            var schedules = _schedules.OrderBy(s => s.NextRunTime);
            return schedules.First();
        }
    }
}
