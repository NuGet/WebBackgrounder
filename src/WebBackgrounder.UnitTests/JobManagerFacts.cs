using System;
using System.Collections.Concurrent;
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
                var jobDoneEvent = new ManualResetEvent(false);
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var jobs = new[] { job.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns(new Task(() => jobDoneEvent.Set()));

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    Assert.True(jobDoneEvent.WaitOne(TimeSpan.FromSeconds(2)));
                }
            }

            [Fact]
            public void GetsTheJobsDoneInTheRightOrder()
            {
                var completed = new ConcurrentQueue<string>();
                var longerJobDoneEvent = new ManualResetEvent(false);
                var shorterJobDoneEvent = new ManualResetEvent(false);
                var longerJob = new Mock<IJob>();
                var shorterJob = new Mock<IJob>();
                longerJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(4));
                shorterJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(3));
                var jobs = new[] { longerJob.Object, shorterJob.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(shorterJob.Object)).Returns((Task)null).Callback(() => { completed.Enqueue("shortJob"); shorterJobDoneEvent.Set(); });
                coordinator.Setup(c => c.GetWork(longerJob.Object)).Returns((Task)null).Callback(() => { completed.Enqueue("longJob"); longerJobDoneEvent.Set(); });

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();

                    Assert.True(longerJobDoneEvent.WaitOne(TimeSpan.FromSeconds(5)));
                    Assert.True(shorterJobDoneEvent.WaitOne(TimeSpan.Zero));
                }

                var ordered = completed.ToArray();
                Assert.Equal("shortJob", ordered[0]);
                Assert.Equal("longJob", ordered[1]);
            }

            [Fact]
            public void DoesNotThrowExceptionWhenThereIsNoWorkToDo()
            {
                var jobNoTask = new Mock<IJob>();
                jobNoTask.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var secondJob = new Mock<IJob>();
                secondJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(2));
                var jobs = new[] { jobNoTask.Object, secondJob.Object };
                var firstJobCompleteEvent = new ManualResetEvent(false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(jobNoTask.Object)).Returns((Task)null);
                coordinator.Setup(c => c.GetWork(secondJob.Object)).Returns((Task)null).Callback(() => firstJobCompleteEvent.Set());

                bool failed = false;

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Fail(e => failed = true);
                    manager.Start();
                    Assert.True(firstJobCompleteEvent.WaitOne(TimeSpan.FromSeconds(1)));
                }

                Assert.False(failed);
            }

            [Fact]
            public void SchedulerRestartsWhenJobFailsAndRestartSchedulerIsTrue()
            {
                var jobNoTask = new Mock<IJob>();
                jobNoTask.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var secondJob = new Mock<IJob>();
                secondJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(2));
                var jobs = new[] { jobNoTask.Object, secondJob.Object };
                var firstJobCompleteEvent = new ManualResetEvent(false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(jobNoTask.Object)).Throws<Exception>();
                coordinator.Setup(c => c.GetWork(secondJob.Object)).Returns((Task)null).Callback(() => firstJobCompleteEvent.Set());

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.RestartSchedulerOnFailure = true;
                    manager.Start();
                    Assert.True(firstJobCompleteEvent.WaitOne(TimeSpan.FromSeconds(1)));
                }
            }

            [Fact]
            public void SchedulerDoesNotRestartWhenJobFailsAndRestartSchedulerIsFalse()
            {
                var jobNoTask = new Mock<IJob>();
                jobNoTask.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var secondJob = new Mock<IJob>();
                secondJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(2));
                var jobs = new[] { jobNoTask.Object, secondJob.Object };
                var firstJobCompleteEvent = new ManualResetEvent(false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(jobNoTask.Object)).Throws<Exception>();
                coordinator.Setup(c => c.GetWork(secondJob.Object)).Returns((Task)null).Callback(() => firstJobCompleteEvent.Set());

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    Assert.False(firstJobCompleteEvent.WaitOne(TimeSpan.FromSeconds(1)));
                }
            }
        }
    }
}
