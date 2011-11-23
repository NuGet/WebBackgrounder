using System;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class WorkItemExtensionsFacts
    {
        public class TheIsActiveMethod
        {
            [Fact]
            public void ReturnsTrueForIncomplete()
            {
                var workItem = new Mock<IWorkItem>();
                workItem.Setup(w => w.Completed).Returns((DateTime?)null);

                bool active = workItem.Object.IsActive();

                Assert.True(active);
            }

            [Fact]
            public void ReturnsFalseForComplete()
            {
                var workItem = new Mock<IWorkItem>();
                workItem.Setup(w => w.Completed).Returns(DateTime.UtcNow);

                bool active = workItem.Object.IsActive();

                Assert.False(active);
            }

            [Fact]
            public void ReturnsFalseForNullWorkItem()
            {
                Assert.False(((IWorkItem)null).IsActive());
            }
        }

        public class TheIsTimedOutMethod
        {
            [Fact]
            public void ReturnsTrueWhenWorkItemIsPastJobExpirationTimespan()
            {
                var started = DateTime.UtcNow.AddSeconds(-10);
                var workItem = new Mock<IWorkItem>();
                workItem.Setup(w => w.Started).Returns(started);
                var job = new Mock<IJob>();
                job.Setup(j => j.Timeout).Returns(TimeSpan.FromSeconds(9));

                bool expired = workItem.Object.IsTimedOut(job.Object);

                Assert.True(expired);
            }

            [Fact]
            public void ReturnsFalseWhenWorkItemIsWithinJobExpirationTimespan()
            {
                var started = DateTime.UtcNow.AddSeconds(-5);
                var workItem = new Mock<IWorkItem>();
                workItem.Setup(w => w.Started).Returns(started);
                var job = new Mock<IJob>();
                job.Setup(j => j.Timeout).Returns(TimeSpan.FromSeconds(100));

                bool expired = workItem.Object.IsTimedOut(job.Object);

                Assert.False(expired);
            }

            [Fact]
            public void ReturnsFalseWhenWorkItemIsNull()
            {
                var expired = ((IWorkItem)null).IsTimedOut(new Mock<IJob>().Object);

                Assert.False(expired);
            }

            [Fact]
            public void ThrowsArgumentNullExceptionWhenJobIsNull()
            {
                Assert.Throws<ArgumentNullException>(() => ((IWorkItem)null).IsTimedOut(null));
            }
        }
    }
}
