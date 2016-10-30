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
                var job =
                    new Task(() =>{ throw new InvalidOperationException("Hey, this is supposed to be shut down!"); })
                    .ToJob();

                host.Stop(true);

                host.DoWork(job);
            }

            [Fact]
            public void WaitsForTaskToComplete()
            {
                var host = new JobHost();
                var workTask = new Task(() => host.DoWork(DelegatingJob.Create( new Task(() =>
                {
                    // Was getting inconsistent results with Thread.Sleep(100)
                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(1);
                    }
                }))));
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

                Assert.DoesNotThrow(() => host.DoWork(DelegatingJob.Create(task)));
            }

            [Fact]
            public void DoesNotCallStartIfWorkIsCanceled()
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.SetException(new Exception());
                var task = tcs.Task;

                var host = new JobHost();
                host.Stop(true);

                Assert.DoesNotThrow(() => host.DoWork(DelegatingJob.Create(task)));
            }

            [Fact]
            public void CancelsJobIfWorkIsCanceled()
            {
                var task = default(Task);
                Func<CancellationToken, Task> thunk = (CancellationToken token) => {
                    task = Task.Factory.StartNew(() => {
                        while (true)
                        {
                            token.ThrowIfCancellationRequested();
                            Thread.Sleep(10);
                        }
                    });
                    return task;
                };

                var host = new JobHost();
                var stopTask = Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(100); // Give time for the host to start the work.
                        host.Stop(true);
                    });
                host.DoWork(DelegatingJob.Create(thunk)); // DoWork waits on the task.
                Assert.True(task.IsCanceled || task.IsFaulted); // This is giving me a task.IsFaulted, shouldn't it be Canceled instead?
                stopTask.Wait();
            }
        }
    }

    public static class TaskExtensions
    {
        public static IJob ToJob(this Task @this)
        {
            return DelegatingJob.Create(@this);
        }
    }
}
