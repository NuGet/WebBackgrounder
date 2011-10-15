using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class ScheduleFacts
    {
        public class TheNextRunTimeProperty
        {
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
                var now = DateTime.UtcNow;
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object, () => now) {LastRunTime = now};

                var interval = schedule.GetIntervalToNextRun();

                Assert.Equal(30, interval.TotalSeconds);
            }

            [Fact]
            public void ReturnsTheSpanBetweenNowAndNextRunTimeFiguringInLastRun()
            {
                var now = DateTime.UtcNow;
                
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object, () => now) { LastRunTime = now.AddSeconds(-20)};

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
                var startDate = DateTime.UtcNow;
                var dates = new Queue<DateTime>(new[]{ startDate, startDate.AddMilliseconds(10) });
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromMilliseconds(30));
                var schedule = new Schedule(job.Object, dates.Dequeue) { LastRunTime = startDate};
                
                var firstInterval = schedule.GetIntervalToNextRun();
                var secondInterval = schedule.GetIntervalToNextRun();

                Assert.Equal(30, firstInterval.TotalMilliseconds);
                Assert.Equal(20, secondInterval.TotalMilliseconds);
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
                var now = DateTime.UtcNow;
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(30));
                var schedule = new Schedule(job.Object, () => now) {LastRunTime = DateTime.UtcNow.AddSeconds(-100)};

                schedule.SetRunComplete();

                Assert.Equal(now, schedule.LastRunTime);
            }
        }
    }
}
