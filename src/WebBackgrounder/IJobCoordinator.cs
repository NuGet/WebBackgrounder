namespace WebBackgrounder
{
    public interface IJobCoordinator
    {
        /// <summary>
        /// Coordinates the work to be done and then does the work if necessary.
        /// </summary>
        /// <param name="job"></param>
        void PerformWork(IJob job);
    }
}
