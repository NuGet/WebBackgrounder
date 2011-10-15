using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace WebBackgrounder.UnitTests
{
    public class SchedulerFacts
    {
        public class TheNextMethod
        {
            [Fact]
            public void ReturnsTheScheduleWithTheLowestRunTime()
            {
                var jobOne = new Mock<IJob>();
                jobOne.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(10));
                var jobTwo = new Mock<IJob>();
                jobTwo.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(7));
                var jobThree = new Mock<IJob>();
                jobThree.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(27));
                var jobs = new[] { jobOne.Object, jobTwo.Object, jobThree.Object };
                var scheduler = new Scheduler(jobs);

                var schedule = scheduler.Next();

                Assert.Equal(jobTwo.Object, schedule.Job);
            }

            [Fact]
            public void ReturnsTheCorrectNextScheduleWhenRunComplete()
            {
                var startDate = DateTime.UtcNow.AddMilliseconds(-7);
                var dates = new Queue<DateTime>(new[]
                {
                    startDate,
                    startDate.AddSeconds(7),
                    startDate.AddSeconds(10),
                    startDate.AddSeconds(14),
                    startDate.AddSeconds(17),
                });

                var jobs = new[] { new WaitJob(10), new WaitJob(7), new WaitJob(17) };
                var scheduler = new Scheduler(jobs, dates.Dequeue);

                var firstSchedule = scheduler.Next();
                firstSchedule.Job.Execute();
                firstSchedule.SetRunComplete();
                var secondSchedule = scheduler.Next();
                secondSchedule.SetRunComplete();
                var thirdSchedule = scheduler.Next();
                
                thirdSchedule.SetRunComplete();
                var fourthSchedule = scheduler.Next();
                fourthSchedule.SetRunComplete();

                Assert.Equal(7, ((WaitJob)firstSchedule.Job).Id);
                Assert.Equal(10, ((WaitJob)secondSchedule.Job).Id);
                Assert.Equal(7, ((WaitJob)thirdSchedule.Job).Id);
                Assert.Equal(17, ((WaitJob)fourthSchedule.Job).Id);
            }

            // If a task takes longer than its interval, 
            // we need to make sure it doesn't block other tasks from running.
            [Fact]
            public void TasksAreScheduledFairly()
            {
                var startDate = DateTime.UtcNow.AddMilliseconds(-7);
                var dates = new Queue<DateTime>(new[]
                {
                    startDate,
                    startDate.AddSeconds(100),
                    startDate.AddSeconds(100),
                    startDate.AddSeconds(140),
                    startDate.AddSeconds(170),
                });

                var jobs = new[] { new WaitJob(10), new WaitJob(7), new WaitJob(17) };
                var scheduler = new Scheduler(jobs, dates.Dequeue);

                var firstSchedule = scheduler.Next();
                firstSchedule.Job.Execute();
                firstSchedule.SetRunComplete();
                var secondSchedule = scheduler.Next();
                secondSchedule.SetRunComplete();
                var thirdSchedule = scheduler.Next();

                thirdSchedule.SetRunComplete();
                var fourthSchedule = scheduler.Next();
                fourthSchedule.SetRunComplete();

                Assert.Equal(7, ((WaitJob)firstSchedule.Job).Id);
                Assert.Equal(10, ((WaitJob)secondSchedule.Job).Id);
                Assert.Equal(17, ((WaitJob)thirdSchedule.Job).Id);
                Assert.Equal(7, ((WaitJob)fourthSchedule.Job).Id);
            }

            private class WaitJob : Job
            {
                public WaitJob(int intervalSeconds) : base("Waits", TimeSpan.FromSeconds(intervalSeconds))
                {
                    Id = intervalSeconds;
                }

                public int Id { get; private set; }

                public override Task Execute()
                {
                    return new Task(() => Thread.Sleep(1));
                }
            }
        }

        public class TheConstructor
        {
            [Fact]
            public void ThrowsExceptionWhenAnyJobHasNegativeInterval()
            {
                var job = new Mock<IJob>();
                job.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(1));
                var negaJob = new Mock<IJob>();
                negaJob.Setup(j => j.Interval).Returns(TimeSpan.FromSeconds(-1));
                var jobs = new[]
                {
                    job.Object,
                    negaJob.Object
                };

                Assert.Throws<ArgumentException>(() => new Scheduler(jobs));
            }
        }
    }
}
