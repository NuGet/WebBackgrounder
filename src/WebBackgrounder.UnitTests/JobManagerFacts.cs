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
                bool jobDone = false;
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var jobs = new[] { job.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns(new Task(() => jobDone = true));

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    WaitForConditionOrTimeout(() => jobDone, TimeSpan.FromSeconds(2));
                }

                Assert.True(jobDone);
            }

            [Fact]
            public void GetsTheJobsDoneInTheRightOrder()
            {
                DateTime jobDone = DateTime.MinValue;
                DateTime anotherJobDone = DateTime.MinValue;
                var job = new Mock<IJob>();
                var anotherJob = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(10));
                anotherJob.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(1));
                var jobs = new[] { job.Object, anotherJob.Object };
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns(new Task(() => jobDone = DateTime.UtcNow));
                coordinator.Setup(c => c.GetWork(anotherJob.Object)).Returns(new Task(() => anotherJobDone = DateTime.UtcNow));

                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Start();
                    WaitForConditionOrTimeout(() => jobDone > DateTime.MinValue, TimeSpan.FromSeconds(2));
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
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.GetWork(job.Object)).Returns((Task)null);

                bool failed = false;
                using (var manager = new JobManager(jobs, coordinator.Object))
                {
                    manager.Fail(e => failed = true);
                    manager.Start();
                    WaitForConditionOrTimeout(() => failed, TimeSpan.FromSeconds(2));
                }

                Assert.False(failed);
            }

            private void WaitForConditionOrTimeout(Func<bool> predicate, TimeSpan waitSpan)
            {
                DateTime start = DateTime.UtcNow;
                while (true)
                {
                    if (predicate() || DateTime.UtcNow > start.Add(waitSpan))
                    {
                        return;
                    }
                    Thread.Sleep(1);
                }
            }
        }
    }
}
