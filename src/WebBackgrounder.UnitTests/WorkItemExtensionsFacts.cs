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
    }
}
