using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class WebFarmJobCoordinatorFacts
    {
        public class TheReserveWorkMethod
        {
            [Fact]
            public void ReturnsNullWhenActiveWorkersExistAndIsNotTimedOut()
            {
                var job = new FakeJob { Name = "jobname", Timeout = TimeSpan.MaxValue };
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.GetLastWorkItem(job)).Returns(new Mock<IWorkItem>().Object);
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", job);

                Assert.Null(unitOfWork);
            }

            [Fact]
            public void ReturnsNullWhenActiveWorkersExistWithinTransaction()
            {
                var job = new FakeJob { Name = "jobname", Timeout = TimeSpan.MaxValue };
                var complete = new Mock<IWorkItem>();
                complete.Setup(wi => wi.Completed).Returns(DateTime.UtcNow);
                var active = new Mock<IWorkItem>();
                active.Setup(wi => wi.Completed).Returns((DateTime?)null);

                var activeWorkerQueue = new Queue<IWorkItem>(new[] { complete.Object, active.Object });
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.GetLastWorkItem(job)).Returns(activeWorkerQueue.Dequeue);
                repository.Setup(r => r.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a());
                repository.Setup(r => r.CreateWorkItem("worker-id", job)).Throws(new InvalidOperationException());
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", job);

                Assert.Null(unitOfWork);
            }

            [Fact]
            public void ReturnsUnitOfWorkWhenNoActiveWorkers()
            {
                var job = new FakeJob { Name = "jobname" };
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.GetLastWorkItem(job)).Returns((IWorkItem)null);
                repository.Setup(r => r.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a());
                repository.Setup(r => r.CreateWorkItem("worker-id", job)).Returns(123);
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", job);

                Assert.NotNull(unitOfWork);
            }

            [Fact]
            public void ReturnsUnitOfWorkWhenLastActiveWorkerIsTimedOut()
            {
                var job = new FakeJob { Name = "jobname", Timeout = TimeSpan.FromSeconds(10) };
                var workItem = new Mock<IWorkItem>();
                workItem.Setup(w => w.Id).Returns(1233);
                workItem.Setup(w => w.Started).Returns(DateTime.UtcNow.AddSeconds(-11));
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.GetLastWorkItem(job)).Returns(workItem.Object);
                repository.Setup(r => r.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a());
                repository.Setup(r => r.CreateWorkItem("worker-id", job)).Returns(123);
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", job);

                Assert.NotNull(unitOfWork);
                repository.Verify(r => r.SetWorkItemFailed(1233, It.IsAny<TimeoutException>()));
            }
        }

        public class TheGetWorkMethod
        {
            [Fact]
            public void WithJobThatThrowsExceptionWhenCreatingTaskStillReturnsTask()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Name).Returns("job-name");
                job.Setup(j => j.Execute()).Throws(new InvalidOperationException("Test Exception"));
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.GetLastWorkItem(job.Object)).Returns((IWorkItem)null);
                repository.Setup(r => r.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a());
                repository.Setup(r => r.CreateWorkItem("worker-id", job.Object)).Returns(() => 123);
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var task = coordinator.GetWork(job.Object);

                Assert.NotNull(task);
            }

            [Fact]
            public void WithNullJobThrowsArgumentNullException()
            {
                var repository = new Mock<IWorkItemRepository>();
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var exception = Assert.Throws<ArgumentNullException>(() => coordinator.GetWork(null));

                Assert.Equal("job", exception.ParamName);
            }
        }

        public class TheCtor
        {
            [Fact]
            public void WithNullWorkItemRepositoryThrowsArgumentNullException()
            {
                var exception = Assert.Throws<ArgumentNullException>(() => new WebFarmJobCoordinator(null));

                Assert.Equal("workItemRepository", exception.ParamName);
            }
        }

        class FakeJob : IJob
        {
            public string Name
            {
                get;
                set;
            }

            public Task Execute()
            {
                return null;
            }

            public TimeSpan Interval
            {
                get;
                set;
            }

            public TimeSpan Timeout
            {
                get;
                set;
            }
        }
    }
}