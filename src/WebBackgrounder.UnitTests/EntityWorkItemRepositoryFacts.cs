using System;
using System.Data.Entity;
using System.Linq;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class EntityWorkItemRepositoryFacts
    {
        public class TheGetLastWorkItemMethod
        {
            [Fact]
            public void ReturnsActiveWorkItemWhenAWorkerHasNoCompletedDate()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Name).Returns("docoolstuffjobname");
                var workItems = new InMemoryDbSet<WorkItem> 
                {
                    new WorkItem
                    {
                        JobName = job.Object.Name, 
                        Started = DateTime.UtcNow, 
                        Completed = null
                    }
                };
                var context = new Mock<WorkItemsContext>();
                context.Object.WorkItems = workItems;
                var repository = new EntityWorkItemRepository(() => context.Object);

                var activeWorkItem = repository.GetLastWorkItem(job.Object);

                Assert.True(activeWorkItem.IsActive());
            }

            [Fact]
            public void ReturnsLastInactiveWorkItemWhenAllWorkItemsHaveCompletedDate()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Name).Returns("docoolstuff");
                var workItems = new InMemoryDbSet<WorkItem> 
                {
                    new WorkItem
                    {
                        Id = 1,
                        JobName = job.Object.Name, 
                        Started = DateTime.UtcNow.AddMinutes(-1), 
                        Completed = DateTime.UtcNow.AddMinutes(-1)
                    },
                    new WorkItem
                    {
                        Id = 2,
                        JobName = job.Object.Name, 
                        Started = DateTime.UtcNow, 
                        Completed = DateTime.UtcNow
                    }
                };
                var context = new Mock<WorkItemsContext>();
                context.Object.WorkItems = workItems;
                var repository = new EntityWorkItemRepository(() => context.Object);

                var activeWorker = repository.GetLastWorkItem(job.Object);

                Assert.Equal(2, activeWorker.Id);
                Assert.False(activeWorker.IsActive());
            }

            [Fact]
            public void ReturnsNullWhenNoWorkItemsReturnedForGivenJob()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Name).Returns("docoolstuff");
                var workItems = new InMemoryDbSet<WorkItem> 
                {
                    new WorkItem
                    {
                        JobName = "DoNotDoAnyCoolStuff", 
                        Started = DateTime.UtcNow, 
                        Completed = DateTime.UtcNow
                    }
                };
                var context = new Mock<WorkItemsContext>();
                context.Object.WorkItems = workItems;
                var repository = new EntityWorkItemRepository(() => context.Object);

                var activeWorkItem = repository.GetLastWorkItem(job.Object);

                Assert.Null(activeWorkItem);
            }
        }

        public class TheCreateWorkItemMethod
        {
            public void CreatesNewWorkItemThatIsNotComplete()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Name).Returns("do-cool-stuff");
                var before = DateTime.UtcNow;
                var context = new Mock<WorkItemsContext>();
                context.Setup(c => c.SaveChanges()).Verifiable();
                context.Object.WorkItems = new InMemoryDbSet<WorkItem>();
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.CreateWorkItem("web-server-1", job.Object);

                var created = context.Object.WorkItems.First(w => w.WorkerId == "web-server-1");
                Assert.NotNull(created);
                Assert.Null(created.Completed);
                Assert.Equal("do-cool-stuff", created.JobName);
                Assert.True(created.Started >= before);
                context.Verify();
            }
        }

        public class TheSetWorkItemCompleteMethod
        {
            [Fact]
            public void SetsWorkItemCompletedSetsCompletedDate()
            {
                const long workItemId = 123;
                var context = new Mock<WorkItemsContext>();
                var workItem = new WorkItem { Id = workItemId };
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(workItemId)).Returns(workItem);
                context.Object.WorkItems = workItems.Object;
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemCompleted(workItemId);

                Assert.NotNull(workItem.Completed);
            }

            [Fact]
            public void SavesChanges()
            {
                var context = new Mock<WorkItemsContext>();
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(It.IsAny<long>())).Returns(new WorkItem());
                context.Object.WorkItems = workItems.Object;
                context.Setup(c => c.SaveChanges()).Verifiable();
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemCompleted(123);

                context.Verify();
            }
        }

        public class TheSetWorkItemFailedMethod
        {
            [Fact]
            public void SetsWorkItemFailedSetsCompletedDateAndExceptionInfo()
            {
                const long workItemId = 123;
                var context = new Mock<WorkItemsContext>();
                var workItem = new WorkItem { Id = workItemId };
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(workItemId)).Returns(workItem);
                context.Object.WorkItems = workItems.Object;
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemFailed(workItemId, new InvalidOperationException("Pretend failure!"));

                Assert.NotNull(workItem.Completed);
                Assert.Contains("Pretend failure!", workItem.ExceptionInfo);
            }

            [Fact]
            public void SavesChanges()
            {
                var context = new Mock<WorkItemsContext>();
                var workItems = new Mock<IDbSet<WorkItem>>();
                workItems.Setup(w => w.Find(It.IsAny<long>())).Returns(new WorkItem());
                context.Object.WorkItems = workItems.Object;
                context.Setup(c => c.SaveChanges()).Verifiable();
                var repository = new EntityWorkItemRepository(() => context.Object);

                repository.SetWorkItemFailed(123, new InvalidOperationException());

                context.Verify();
            }
        }
    }
}
