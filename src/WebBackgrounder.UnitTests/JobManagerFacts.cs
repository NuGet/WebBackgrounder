using System;

using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class JobManagerFacts
    {
        public class TheConstructor
        {
            [Fact]
            public void ThrowsExceptionWhenAnyParameterIsNull()
            {
                Assert.Throws<ArgumentNullException>(() => new JobManager(null, new Mock<IJobHost>().Object));
                Assert.Throws<ArgumentNullException>(() => new JobManager(new[] { new Mock<IJob>().Object }, (IJobHost)null));
                Assert.Throws<ArgumentNullException>(() => new JobManager(new[] { new Mock<IJob>().Object }, (IJobCoordinator)null));
            }
        }

        public class TheStartMethod
        {
            [Fact]
            public void GetsTheJobDone()
            {
                var resetEvent = new ManualResetEvent(initialState: false);
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var jobs = new[] { job.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns(new Task(() => resetEvent.Set()));

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    Assert.True(resetEvent.WaitOne(timeout: TimeSpan.FromSeconds(2)));
                }
            }

            [Fact]
            public void GetsTheJobsDoneInTheRightOrder()
            {
                var jobOneEvent = new ManualResetEvent(initialState: false);
                var jobTwoEvent = new ManualResetEvent(initialState: false);
                DateTime jobDone = DateTime.MinValue;
                DateTime anotherJobDone = DateTime.MinValue;
                var job = new Mock<IJob>();
                var anotherJob = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(10));
                anotherJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var jobs = new[] { job.Object, anotherJob.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns(new Task(() =>
                {
                    jobDone = DateTime.UtcNow;
                    jobOneEvent.Set();
                }));
                coordinator.Setup(c => c.GetWork(anotherJob.Object)).Returns(new Task(() =>
                {
                    anotherJobDone = DateTime.UtcNow;
                    jobTwoEvent.Set();
                }));

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    Assert.True(jobOneEvent.WaitOne(timeout: TimeSpan.FromSeconds(4)));
                    Assert.True(jobTwoEvent.WaitOne(timeout: TimeSpan.FromSeconds(4)));
                }

                Assert.True(jobDone > DateTime.MinValue);
                Assert.True(anotherJobDone > DateTime.MinValue);
                Assert.True(anotherJobDone < jobDone);
            }

            [Fact]
            public void DoesNotThrowExceptionWhenThereIsNoWorkToDo()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var jobs = new[] { job.Object };
                var jobCalledEvent = new ManualResetEvent(initialState: false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns((Task)null).Callback(() => jobCalledEvent.Set());
                bool failed = false;

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Fail(e => failed = true);
                    manager.Start();
                    Assert.True(jobCalledEvent.WaitOne(timeout: TimeSpan.FromSeconds(2)));
                }

                Assert.False(failed);
            }
        }
    }
}
