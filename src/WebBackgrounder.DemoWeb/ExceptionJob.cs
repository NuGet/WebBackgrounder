using System;
using System.Threading.Tasks;

namespace WebBackgrounder.DemoWeb
{
    public class ExceptionJob : Job
    {
        public ExceptionJob(TimeSpan interval)
            : base("Sample Job", interval)
        {
        }

        public override Task Execute()
        {
            return new Task(() => { throw new InvalidOperationException("This is a test"); });
        }
    }
}