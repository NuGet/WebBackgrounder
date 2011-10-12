using System;

namespace WebBackgrounder.EntityFramework {
    public interface IJobRepository {
        /// <summary>
        /// Returns true if the specified job is currently pending or running.
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        bool PendingJobsExist(string jobName);

        /// <summary>
        /// Creates a request from a worker that it's ready to take on a 
        /// job. It might not necessarily get it.
        /// </summary>
        void CreateJobRequest(string jobName, Guid workerId);

        /// <summary>
        /// Returns the worker Id that gets to do the job.
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        Guid GetWorkerIdForJob(string jobName);

        /// <summary>
        /// Indicates that the worker is about to start.
        /// </summary>
        /// <param name="jobName"></param>
        void StartWork(string jobName, Guid workerId);

        /// <summary>
        /// Marks the job as complete.
        /// </summary>
        /// <param name="jobName"></param>
        void CompleteJob(string jobName, Guid workerId);
    }
}
