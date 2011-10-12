using System;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests {
    public class TaskManagerFacts {
        public class TheRunTaskMethod {
            [Fact]
            public void DoesNotRunTaskIfHostIsShuttingDown() {
                var job = new Mock<IJob>();
                var host = new Mock<IJobHost>();
                host.Setup(h => h.ShuttingDown).Returns(true);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.PerformWork(It.IsAny<IJob>())).Throws(new InvalidOperationException("Should not try to do any work"));
                
                var taskManager = new JobWorkersManager(job.Object, host.Object, coordinator.Object);

                taskManager.RunTask(job.Object);
            }

            [Fact]
            public void AttemptsToRunTaskIfHostIsNotShuttingDown() {
                var job = new Mock<IJob>();
                var host = new Mock<IJobHost>();
                host.Setup(h => h.ShuttingDown).Returns(false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.PerformWork(job.Object)).Verifiable();
                var taskManager = new JobWorkersManager(job.Object, host.Object, coordinator.Object);

                taskManager.RunTask(job.Object);

                coordinator.Verify();
            }
        }
    }
}
