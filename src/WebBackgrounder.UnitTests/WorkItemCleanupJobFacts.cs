using System;
using System.Linq;
using Moq;
using WebBackgrounder.Jobs;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class WorkItemCleanupJobFacts
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void DeletesItemsOlderThanSpecifiedTimeSpan()
            {
                var context = new Mock<IWorkItemsContext>();
                context.Setup(c => c.SaveChanges()).Verifiable();
                context.Setup(c => c.WorkItems).Returns(new InMemoryDbSet<WorkItem>
                {
                    new WorkItem {Id = 101, Completed = DateTime.UtcNow.AddDays(-4)}, 
                    new WorkItem {Id = 102, Completed = DateTime.UtcNow.AddDays(-4)}, 
                    new WorkItem {Id = 103, Completed = DateTime.UtcNow.AddDays(-2).AddMilliseconds(-1)}, 
                    new WorkItem {Id = 104, Completed = DateTime.UtcNow}, 
                    new WorkItem {Id = 105 }
                });
                var job = new WorkItemCleanupJob(TimeSpan.FromMilliseconds(1), TimeSpan.FromDays(2), context.Object);
                var task = job.Execute();
                task.Start();
                task.Wait();

                Assert.Equal(2, context.Object.WorkItems.Count());
                Assert.Equal(104, context.Object.WorkItems.First().Id);
                Assert.Equal(105, context.Object.WorkItems.ElementAt(1).Id);
                context.Verify();
            }

            [Fact]
            public void DoesNothingWhenAllRecordsAreWithinKeepRecordsSpan()
            {
                var context = new Mock<IWorkItemsContext>();
                context.Setup(c => c.SaveChanges()).Throws(
                    new InvalidOperationException("Should not have tried to save changes"));
                context.Object.WorkItems = new InMemoryDbSet<WorkItem> { new WorkItem(), new WorkItem() };
                var job = new WorkItemCleanupJob(TimeSpan.FromSeconds(1), TimeSpan.FromDays(1), context.Object);

                job.Execute();
            }
        }
    }
}
