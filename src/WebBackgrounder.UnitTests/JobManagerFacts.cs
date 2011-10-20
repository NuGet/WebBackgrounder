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
                DateTime jobOneDone = DateTime.MinValue;
                DateTime jobTwoDone = DateTime.MinValue;
                var jobOne = new Mock<IJob>();
                jobOne.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(10));
                jobOne.Setup(j => j.Name).Returns("Job One");
                var jobTwo = new Mock<IJob>();
                jobTwo.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(8));
                jobTwo.Setup(j => j.Name).Returns("Job Two");
                var jobs = new[] { jobOne.Object, jobTwo.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(jobOne.Object)).Returns(new Task(() =>
                {
                    jobOneDone = DateTime.UtcNow;
                    jobOneEvent.Set();
                }));
                coordinator.Setup(c => c.GetWork(jobTwo.Object)).Returns(new Task(() =>
                {
                    jobTwoDone = DateTime.UtcNow;
                    jobTwoEvent.Set();
                }));

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    Assert.True(jobOneEvent.WaitOne(timeout: TimeSpan.FromSeconds(4)));
                    Assert.True(jobTwoEvent.WaitOne(timeout: TimeSpan.FromSeconds(4)));
                }

                Assert.True(jobOneDone > DateTime.MinValue);
                Assert.True(jobTwoDone > DateTime.MinValue);
                Assert.True(jobTwoDone < jobOneDone);
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
