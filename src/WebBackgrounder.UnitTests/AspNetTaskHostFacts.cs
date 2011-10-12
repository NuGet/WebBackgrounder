
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
namespace WebBackgrounder.UnitTests {
    public class AspNetTaskHostFacts {
        public class TheStopMethod {
            [Fact]
            public void SetsShuttingDownToTrue() {
                var host = new AspNetTaskHost();

                host.Stop(true);

                Assert.True(host.ShuttingDown);
            }

            [Fact]
            public void WaitsForTaskToComplete() {
                var host = new AspNetTaskHost();
                DateTime after;
                DateTime afterStop = DateTime.MinValue;
                var task = new Task(() => {
                    host.Stop(true);
                    afterStop = DateTime.UtcNow;
                });

                lock (host) {
                    task.Start();
                    after = DateTime.UtcNow;
                    Thread.Sleep(500);
                }
                task.Wait();

                Assert.True(afterStop >= after.AddMilliseconds(500));
            }
        }
    }
}
