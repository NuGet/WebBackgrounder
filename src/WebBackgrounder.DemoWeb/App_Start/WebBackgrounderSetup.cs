using System;
using System.Web;
using Elmah;
using WebBackgrounder.Jobs;

[assembly: WebActivator.PostApplicationStartMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Shutdown")]

namespace WebBackgrounder.DemoWeb.App_Start
{
    public static class WebBackgrounderSetup
    {
        static readonly JobManager _jobManager = CreateJobWorkersManager();
        static HttpApplication _httpApplication = null;

        public static void Start()
        {
            _jobManager.Start();
        }

        public static void Shutdown()
        {
            _jobManager.Dispose();
        }

        public static void SetHttpApplication(HttpApplication httpApplication)
        {
            _httpApplication = httpApplication;
        }

        private static JobManager CreateJobWorkersManager()
        {
            var jobs = new IJob[]
            {
                new SampleJob(TimeSpan.FromSeconds(5)),
                new WorkItemCleanupJob(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), new WorkItemsContext())
            };

            var coordinator = new WebFarmJobCoordinator(new EntityWorkItemRepository(() => new WorkItemsContext()));
            var manager = new JobManager(jobs, coordinator);
            if (_httpApplication != null)
            {
                manager.Fail(e => ErrorSignal.Get(_httpApplication).Raise(e));
            }
            return manager;
        }
    }
}
