using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebBackgrounder.DemoWeb
{
    public class SampleJob : Job
    {
        public SampleJob(TimeSpan interval) : base("Sample Job", interval)
        {
        }

        public override Task Execute()
        {
            return new Task(() => Thread.Sleep(3000));
        }
    }
}