using System;
using System.Threading;

namespace WebBackgrounder.DemoWeb
{
    public class SampleJob : Job
    {
        public SampleJob(TimeSpan interval) : base("Sample Job", interval)
        {
        }

        public override void Execute()
        {
            Thread.Sleep(3000);
        }
    }
}