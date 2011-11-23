using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebBackgrounder.DemoWeb
{
    public class SampleJob : Job
    {
        public SampleJob(TimeSpan interval, TimeSpan timeout)
            : base("Sample Job", interval, timeout)
        {
        }

        public override Task Execute()
        {
            return new Task(() => Thread.Sleep(3000));
        }
    }
}