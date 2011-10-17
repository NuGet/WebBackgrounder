using System;
using Elmah;
using WebBackgrounder.EntityFramework;
using WebBackgrounder.EntityFramework.Entities;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Shutdown")]

namespace WebBackgrounder.DemoWeb.App_Start
{
    public static class WebBackgrounderSetup
    {
        private static readonly JobManager Manager = CreateJobWorkersManager();

        public static void Start()
        {
            Manager.Start();
        }

        public static void Shutdown()
        {
            Manager.Stop();
        }

        private static JobManager CreateJobWorkersManager()
        {
            var jobs = new IJob[]
            {
                new SampleJob(TimeSpan.FromSeconds(5)),
                new WorkItemCleanupJob(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), new WorkItemsContext())
            };

            Func<string, IWorkItemRepository> repositoryThunk = jobname => new EntityWorkItemRepository(jobname, () => new WorkItemsContext());
            var coordinator = new WebFarmJobCoordinator(repositoryThunk);
            var manager = new JobManager(jobs, coordinator);
            manager.Fail(e => ErrorSignal.FromCurrentContext().Raise(e));
            return manager;
        }
    }
}
