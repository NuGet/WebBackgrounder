using System.Threading.Tasks;

namespace WebBackgrounder
{
    public class SingleServerJobCoordinator : IJobCoordinator
    {
        public Task GetWork(IJob job)
        {
            return job.Execute();
        }

        public void Dispose()
        {
        }
    }
}
