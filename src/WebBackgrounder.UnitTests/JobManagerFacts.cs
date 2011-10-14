using System;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests {
    public class JobManagerFacts {
        public class TheRunTaskMethod {
            [Fact]
            public void DoesNotRunTaskIfHostIsShuttingDown() {
                var job = new Mock<IJob>();
                var host = new Mock<IJobHost>();
                host.Setup(h => h.ShuttingDown).Returns(true);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.PerformWork(It.IsAny<IJob>())).Throws(new InvalidOperationException("Should not try to do any work"));
                
                var taskManager = new JobManager(new[]{ job.Object }, host.Object, coordinator.Object);

                taskManager.RunJob(job.Object);
            }

            [Fact]
            public void AttemptsToRunTaskIfHostIsNotShuttingDown() {
                var job = new Mock<IJob>();
                var host = new Mock<IJobHost>();
                host.Setup(h => h.ShuttingDown).Returns(false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.PerformWork(job.Object)).Verifiable();
                var taskManager = new JobManager(new[] { job.Object }, host.Object, coordinator.Object);

                taskManager.RunJob(job.Object);

                coordinator.Verify();
            }
        }

        public class TheConstructor
        {
            [Fact]
            public void ThrowsExceptionWhenAnyParameterIsNull()
            {
                Assert.Throws<ArgumentNullException>(() => new JobManager(null, new Mock<IJobHost>().Object));
                Assert.Throws<ArgumentNullException>(() => new JobManager(new[] { new Mock<IJob>().Object}, (IJobHost)null));
                Assert.Throws<ArgumentNullException>(() => new JobManager(new[] { new Mock<IJob>().Object }, (IJobCoordinator)null));
            }
        }
    }
}
