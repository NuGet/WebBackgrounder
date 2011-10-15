using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    public abstract class Job : IJob
    {
        protected Job(string name, TimeSpan interval)
        {
            Name = name;
            Interval = interval;
        }

        protected Job(string name, int intervalInSeconds)
        {
            Name = name;
            Interval = TimeSpan.FromSeconds(intervalInSeconds);
        }

        public string Name
        {
            get; 
            private set;
        }

        public abstract Task Execute();

        public TimeSpan Interval 
        { 
            get; private set; 
        }
    }
}
