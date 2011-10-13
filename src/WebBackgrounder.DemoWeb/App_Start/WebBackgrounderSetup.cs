using System;
using WebBackgrounder.EntityFramework;
using WebBackgrounder.EntityFramework.Entities;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Shutdown")]

namespace WebBackgrounder.DemoWeb.App_Start {
    public static class WebBackgrounderSetup
    {
        private static readonly JobWorkersManager Manager = CreateJobWorkersManager();

        public static void Start() {
            Manager.Start();
        }

        public static void Shutdown() {
            Manager.Stop();
        }

        private static JobWorkersManager CreateJobWorkersManager()
        {
            var job = new SampleJob();
            Func<string, IWorkItemRepository> repositoryThunk = (jobname) => new EntityWorkItemRepository(jobname, () => new WorkItemsContext());
            var coordinator = new WebFarmJobCoordinator(repositoryThunk);
            return new JobWorkersManager(job, coordinator);
        }
    }
}
