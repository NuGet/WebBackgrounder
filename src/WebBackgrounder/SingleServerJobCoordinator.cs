using System.Threading.Tasks;

namespace WebBackgrounder
{
    public class SingleServerJobCoordinator : IJobCoordinator
    {
        public IJob GetWork(IJob job)
        {
            return job;
        }

        public void Dispose()
        {
        }
    }
}
