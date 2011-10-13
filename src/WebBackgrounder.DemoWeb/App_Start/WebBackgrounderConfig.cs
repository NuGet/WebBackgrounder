using WebBackgrounder.EntityFramework;
using WebBackgrounder.EntityFramework.Entities;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderConfig), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderConfig), "Shutdown")]

namespace WebBackgrounder.DemoWeb.App_Start {
    public static class WebBackgrounderConfig {
        readonly static JobWorkersManager _manager = new JobWorkersManager(new SampleJob(), new AspNetTaskHost(), new WebFarmJobCoordinator(new JobsContext()));

        public static void Start() {
            _manager.Start();
        }

        public static void Shutdown() {
            _manager.Stop();
        }
    }
}
