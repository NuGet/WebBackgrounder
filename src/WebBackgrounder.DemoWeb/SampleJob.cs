using System;
using System.Threading;

namespace WebBackgrounder.DemoWeb {
    public class SampleJob : IJob {
        public string Name {
            get {
                return "SampleJob";
            }
        }

        public void Execute() {
            Thread.Sleep(3000);
        }

        public TimeSpan Interval {
            get {
                return TimeSpan.FromSeconds(5);
            }
        }
    }
}