using System;
using Elmah;
using WebBackgrounder.Jobs;

[assembly: WebActivator.PostApplicationStartMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Shutdown")]

namespace WebBackgrounder.DemoWeb.App_Start
{
    public static class WebBackgrounderSetup
    {
        static readonly JobManager _jobManager = CreateJobWorkersManager();

        public static void Start()
        {
            _jobManager.Start();
        }

        public static void Shutdown()
        {
            _jobManager.Dispose();
        }

        private static JobManager CreateJobWorkersManager()
        {
            var jobs = new IJob[]
            {
                new SampleJob(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20)),
                /* new ExceptionJob(TimeSpan.FromSeconds(15)), */
                new WorkItemCleanupJob(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), new WorkItemsContext())
            };

            var coordinator = new WebFarmJobCoordinator(new EntityWorkItemRepository(() => new WorkItemsContext()));
            var manager = new JobManager(jobs, coordinator);
            manager.Fail(ex => Elmah.ErrorLog.GetDefault(null).Log(new Error(ex)));
            return manager;
        }
    }
}
