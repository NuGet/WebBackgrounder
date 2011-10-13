using WebBackgrounder.EntityFramework;
using WebBackgrounder.EntityFramework.Entities;

[assembly: WebActivator.PreApplicationStartMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(WebBackgrounder.DemoWeb.App_Start.WebBackgrounderSetup), "Shutdown")]

namespace WebBackgrounder.DemoWeb.App_Start {
    public static class WebBackgrounderSetup {
        readonly static JobWorkersManager Manager = new JobWorkersManager(new SampleJob(), new AspNetTaskHost(), new WebFarmJobCoordinator());

        public static void Start() {
            Manager.Start();
        }

        public static void Shutdown() {
            Manager.Stop();
        }
    }
}
