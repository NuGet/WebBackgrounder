using System;
using System.Threading;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class ScheduleFacts
    {
        public class TheNextRunTimeProperty
        {
            [Fact]
            public void ReturnsIntervalAddedToNowWhenLastRunTimeIsNull()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object);
                var now = DateTime.UtcNow;

                var next = schedule.NextRunTime;

                Assert.True(next >= now.AddSeconds(30));
                Assert.True(next < now.AddSeconds(31));
            }

            [Fact]
            public void ReturnsIntervalAddedToLastRunTime()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(32));
                var lastRunTime = DateTime.UtcNow.AddSeconds(-10);
                var schedule = new Schedule(job.Object) {LastRunTime = lastRunTime};

                var next = schedule.NextRunTime;

                Assert.Equal(lastRunTime.AddSeconds(32), next);
            }
        }

        public class TheGetIntervalToNextRunMethod
        {
            [Fact]
            public void ReturnsTheSpanBetweenNowAndNextRunTime()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object);

                var interval = schedule.GetIntervalToNextRun();

                Assert.Equal(30, interval.TotalSeconds);
            }

            [Fact]
            public void ReturnsTheSpanBetweenNowAndNextRunTimeFiguringInLastRun()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object) { LastRunTime = DateTime.UtcNow.AddSeconds(-20)};

                var interval = schedule.GetIntervalToNextRun();

                Assert.Equal(10, interval.TotalSeconds);
            }

            [Fact]
            public void ReturnsOneMillisecondTimeSpanWhenNextRunIsInThePast()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object) { LastRunTime = DateTime.UtcNow.AddSeconds(-40) };

                var interval = schedule.GetIntervalToNextRun();

                Assert.Equal(1, interval.TotalMilliseconds);
            }

            [Fact]
            public void ShrinksOverTimeEvenWhenLastRunIsNull()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(30));
                var schedule = new Schedule(job.Object);
                schedule.GetIntervalToNextRun();
                Thread.Sleep(10);
                var interval = schedule.GetIntervalToNextRun();
                Assert.True(interval.TotalMilliseconds <= 30);
            }
        }

        public class TheConstructor
        {
            [Fact]
            public void SetsTheJob()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                
                var schedule = new Schedule(job.Object);
                
                Assert.Equal(job.Object, schedule.Job);
            }

            [Fact]
            public void ThrowsArgumentNullExceptionForNullJob()
            {
                Assert.Throws<ArgumentNullException>(() => new Schedule(null));
            }
        }

        public class TheDisposeMethod
        {
            [Fact]
            public void SetsLastRunTimeToNow()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object) {LastRunTime = DateTime.UtcNow.AddSeconds(-100)};
                var now = DateTime.UtcNow;

                ((IDisposable)schedule).Dispose();

                Assert.True(schedule.LastRunTime >= now);
                Assert.True(schedule.LastRunTime <= DateTime.UtcNow);
            }
        }
    }
}
