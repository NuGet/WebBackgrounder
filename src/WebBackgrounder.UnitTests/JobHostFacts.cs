using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class JobHostFacts
    {
        public class TheStopMethod
        {
            [Fact]
            public void EnsuresNoWorkIsDone()
            {
                var host = new JobHost();
                var task = new Task(() => { throw new InvalidOperationException("Hey, this is supposed to be shut down!"); });

                host.Stop(true);

                host.DoWork(task);
            }

            [Fact]
            public void WaitsForTaskToComplete()
            {
                var host = new JobHost();
                var workTask = new Task(() => host.DoWork(new Task(() =>
                {
                    // Was getting inconsistent results with Thread.Sleep(100)
                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(1);
                    }
                })));
                var beforeStop = DateTime.UtcNow;
                workTask.Start();
                while (workTask.Status != TaskStatus.Running)
                {
                    Thread.Sleep(1);
                }

                host.Stop(false);
                var afterStop = DateTime.UtcNow;

                // If Stop didn't wait, we'd expect after to be less than 100 ms larger than beforeStop
                Assert.True((afterStop - beforeStop).TotalMilliseconds >= 100);
            }
        }

        public class TheDoWorkMethod
        {
            [Fact]
            public void ThrowsArgumentNullExceptionIfWorkIsNull()
            {
                var host = new JobHost();

                var exception = Assert.Throws<ArgumentNullException>(() => host.DoWork(null));

                Assert.Equal("work", exception.ParamName);
            }

            [Fact]
            public void DoesNotCallStartIfWorkIsAlreadyScheduledOrCompleted()
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);

                var task = tcs.Task;

                var host = new JobHost();

                Assert.DoesNotThrow(() => host.DoWork(task));
            }
        }
    }
}
