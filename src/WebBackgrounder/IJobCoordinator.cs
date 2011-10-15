using System.Threading.Tasks;

namespace WebBackgrounder
{
    public interface IJobCoordinator
    {
        /// <summary>
        /// Coordinates the work to be done and returns a task embodying that work.
        /// </summary>
        /// <param name="job"></param>
        Task PerformWork(IJob job);
    }
}
