using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            public void ReturnsTheCorrectNextScheduleWhenDisposed()
            {
                var jobs = new[] { new WaitJob(10), new WaitJob(7), new WaitJob(17) };
                var scheduler = new Scheduler(jobs);

                using (var firstSchedule = scheduler.Next())
                {
                    firstSchedule.Job.Execute();
                }
                
                var secondSchedule = scheduler.Next();
                Thread.Sleep(5);
                ((IDisposable)secondSchedule).Dispose();
                var thirdSchedule = scheduler.Next();
                Thread.Sleep(5);
                ((IDisposable)thirdSchedule).Dispose();
                var fourthSchedule = scheduler.Next();
                Thread.Sleep(5);
                ((IDisposable)fourthSchedule).Dispose();
                
                Assert.Equal(jobs[0], secondSchedule.Job);
                Assert.Equal(jobs[1], thirdSchedule.Job);
                Assert.Equal(jobs[2], fourthSchedule.Job);
            }

            // If a task takes longer than its interval, 
            // we need to make sure it doesn't block other tasks from running.
            [Fact]
            public void TasksAreScheduledFairly()
            {
                var now = DateTime.UtcNow;
                var schedules = new[]
                {
                    new Schedule(new WaitJob(2)) { LastRunTime = now.AddDays(-10)},
                    new Schedule(new WaitJob(7)) { LastRunTime = now.AddDays(-20)},
                    new Schedule(new WaitJob(3)) { LastRunTime = now.AddDays(-18)}
                } ;
                var scheduler = new Scheduler(schedules);
                var scheduled = new List<Schedule>();
                scheduled.Add(scheduler.Next()); // waitjob-7ms
                scheduled.Last().LastRunTime = now.AddDays(-9);
                scheduled.Add(scheduler.Next()); // waitjob-3ms
                scheduled.Last().LastRunTime = now.AddDays(-7);
                scheduled.Add(scheduler.Next()); // waitjob-2ms
                scheduled.Last().LastRunTime = now.AddDays(-4);


                Assert.Equal(schedules[1], scheduled[0]);
                Assert.Equal(schedules[2], scheduled[1]);
                Assert.Equal(schedules[0], scheduled[2]);
            }

            private class WaitJob : Job
            {
                public WaitJob(int intervalMilliseconds) : base("Waits", TimeSpan.FromMilliseconds(intervalMilliseconds))
                {
                }

                public override void Execute()
                {
                    Thread.Sleep(Interval);
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
