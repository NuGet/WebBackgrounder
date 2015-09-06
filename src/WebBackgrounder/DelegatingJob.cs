using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    public class DelegatingJob : IJob
    {
        private Func<Task> _taskThunk;

        private DelegatingJob(Func<Task> taskThunk)
        {
            _taskThunk = taskThunk;
        }

        public DelegatingJob(IJob job, Func<Task> taskThunk)
        {
            Name = job.Name;
            Interval = job.Interval;
            Timeout = job.Timeout;
            _taskThunk = taskThunk;
        }

        // Used in unit tests.
        public static IJob Create(Func<Task> taskThunk)
        {
            return new DelegatingJob(taskThunk);
        }

        // Used in unit tests.
        public static IJob Create(Task task)
        {
            return new DelegatingJob(() => task);
        }

        public string Name { get; set; }

        public Task Execute()
        {
            return _taskThunk();
        }

        public TimeSpan Interval { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}