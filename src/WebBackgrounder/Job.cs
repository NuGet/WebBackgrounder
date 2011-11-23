using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    public abstract class Job : IJob
    {
        protected Job(string name, TimeSpan interval, TimeSpan timeout)
        {
            Name = name;
            Interval = interval;
            Timeout = timeout;
        }

        protected Job(string name, TimeSpan interval) : this(name, interval, TimeSpan.MaxValue) { }

        public string Name
        {
            get;
            private set;
        }

        public abstract Task Execute();

        public TimeSpan Interval
        {
            get;
            private set;
        }

        public TimeSpan Timeout
        {
            get;
            private set;
        }
    }
}