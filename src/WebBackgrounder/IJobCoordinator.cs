using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    public interface IJobCoordinator : IDisposable
    {
        /// <summary>
        /// Coordinates the work to be done and returns another job that wraps 
        /// the work as well as the coordination of that work.
        /// </summary>
        /// <param name="job"></param>
        IJob GetWork(IJob job);
    }
}
