 namespace WebBackgrounder
{
    public class SingleServerJobCoordinator : IJobCoordinator
    {
        public void PerformWork(IJob job)
        {
            job.Execute();
        }
    }
}
