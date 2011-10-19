
using System;
using System.Collections.Generic;
using Moq;
using Xunit;
namespace WebBackgrounder.UnitTests
{
    public class WebFarmJobCoordinatorFacts
    {
        public class TheReserveWorkMethod
        {
            [Fact]
            public void ReturnsNullWhenActiveWorkersExist()
            {
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.AnyActiveWorker("jobname")).Returns(true);
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", "jobname");

                Assert.Null(unitOfWork);
            }

            [Fact]
            public void ReturnsNullWhenActiveWorkersExistWithinTransaction()
            {
                var repository = new Mock<IWorkItemRepository>();
                var activeWorkerQueue = new Queue<bool>(new[] { false, true });
                repository.Setup(r => r.AnyActiveWorker("jobname")).Returns(activeWorkerQueue.Dequeue);
                repository.Setup(r => r.AnyActiveWorker("jobname")).Returns(activeWorkerQueue.Dequeue);
                repository.Setup(r => r.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a());
                repository.Setup(r => r.CreateWorkItem("worker-id", "jobname")).Throws(new InvalidOperationException());
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", "jobname");

                Assert.Null(unitOfWork);
            }

            [Fact]
            public void ReturnsUnitOfWorkWhenNoActiveWorkers()
            {
                var repository = new Mock<IWorkItemRepository>();
                repository.Setup(r => r.AnyActiveWorker("jobname")).Returns(false);
                repository.Setup(r => r.RunInTransaction(It.IsAny<Action>())).Callback<Action>(a => a());
                repository.Setup(r => r.CreateWorkItem("worker-id", "jobname")).Returns(123);
                var coordinator = new WebFarmJobCoordinator(repository.Object);

                var unitOfWork = coordinator.ReserveWork("worker-id", "jobname");

                Assert.NotNull(unitOfWork);
            }
        }

        public class TheGetWorkMethod
        {
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
    }
}
