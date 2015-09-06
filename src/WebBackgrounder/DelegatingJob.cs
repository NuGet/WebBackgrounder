using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    public class DelegatingJob : IJob
    {
        private Func<CancellationToken, Task> _taskThunk;

        private DelegatingJob(Func<CancellationToken, Task> taskThunk)
        {
            _taskThunk = taskThunk;
        }

        public DelegatingJob(IJob job, Func<CancellationToken, Task> taskThunk)
        {
            Name = job.Name;
            Interval = job.Interval;
            Timeout = job.Timeout;
            _taskThunk = taskThunk;
        }

        // Used in unit tests.
        public static IJob Create(Func<Task> taskThunk)
        {
            return new DelegatingJob((_) => taskThunk());
        }

        // Used in unit tests.
        public static IJob Create(Task task)
        {
            return new DelegatingJob((_) => task);
        }

        public string Name { get; set; }

        public Task Execute(CancellationToken cancellationToken)
        {
            return _taskThunk(cancellationToken);
        }

        public TimeSpan Interval { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}