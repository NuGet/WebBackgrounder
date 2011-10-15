 using System.Threading.Tasks;

namespace WebBackgrounder
{
    public class SingleServerJobCoordinator : IJobCoordinator
    {
        public Task PerformWork(IJob job)
        {
            return job.Execute();
        }
    }
}
