using System;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests {
    public class TaskManagerFacts {
        public class TheRunTaskMethod {
            [Fact]
            public void DoesNotRunTaskIfHostIsShuttingDown() {
                var task = new Mock<ITask>();
                task.Setup(t => t.Execute()).Throws(new InvalidOperationException("Task should not have been executed because the app was shutting down"));
                var host = new Mock<ITaskHost>();
                host.Setup(h => h.ShuttingDown).Returns(true);
                var taskManager = new TaskManager(null, host.Object, null);

                taskManager.RunTask(task.Object);
            }

            [Fact]
            public void DoesNotRunTaskIfCoordinatorHasNoWork() {
                var task = new Mock<ITask>();
                task.Setup(t => t.Execute()).Throws(new InvalidOperationException("Task should not have been executed because the coordinator said no!"));
                var host = new Mock<ITaskHost>();
                host.Setup(h => h.ShuttingDown).Returns(false);
                var coordinator = new Mock<IJobCoordinator>();
                coordinator.Setup(c => c.CanDoWork(It.IsAny<string>(), It.IsAny<Guid>())).Returns(false);
                var taskManager = new TaskManager(null, host.Object, coordinator.Object);

                taskManager.RunTask(task.Object);
            }
        }
    }
}
