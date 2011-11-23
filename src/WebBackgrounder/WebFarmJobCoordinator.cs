using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator
    {
        readonly static string WebServerWorkerId = Guid.NewGuid().ToString();
        readonly IWorkItemRepository _workItemRepository;

        public WebFarmJobCoordinator(IWorkItemRepository workItemRepository)
        {
            if (workItemRepository == null)
            {
                throw new ArgumentNullException("workItemRepository");
            }
            _workItemRepository = workItemRepository;
        }

        // Returns a task with the work to do if work is available to do and another web server 
        // in the web farm isn't already doing it. 
        public Task GetWork(IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            var unitOfWork = ReserveWork(WebServerWorkerId, job);
            if (unitOfWork == null)
            {
                return null;
            }

            Task task = null;
            try
            {
                task = job.Execute();
            }
            catch (Exception e)
            {
                task = new Task(() => { throw e; });
            }
            task.ContinueWith(c =>
            {
                if (c.IsFaulted)
                {
                    unitOfWork.Fail(c.Exception.GetBaseException());
                }
                else
                {
                    unitOfWork.Complete();
                }
            });

            return task;
        }

        // if work is available to do and another web server in the web farm isn't already doing, 
        // this returns a unit of work used to wrap the work. 
        public JobUnitOfWork ReserveWork(string workerId, IJob job)
        {
            long? workItemId = null;

            // We do a double check here because this may be the first DB query that runs when starting 
            // WebBackgrounder. For those using EF Code First, this could trigger a database creation 
            // (typically on a dev box, hopefully not in production). A database can't be created inside 
            // a transaction scope so we run this query here first.
            var lastWorkItem = _workItemRepository.GetLastWorkItem(job);
            if (lastWorkItem.IsActive() && !lastWorkItem.IsTimedOut(job))
            {
                return null;
            }

            _workItemRepository.RunInTransaction(() =>
                {
                    lastWorkItem = _workItemRepository.GetLastWorkItem(job);

                    if (lastWorkItem.IsTimedOut(job))
                    {
                        lastWorkItem.Completed = DateTime.UtcNow;
                        _workItemRepository.SetWorkItemFailed(lastWorkItem.Id, new TimeoutException("Workitem expired. Job timeout was " + job.Timeout));
                        lastWorkItem = null;
                    }

                    if (lastWorkItem.IsActive())
                    {
                        workItemId = null;
                        return;
                    }
                    workItemId = _workItemRepository.CreateWorkItem(workerId, job);
                }
            );

            if (workItemId == null)
            {
                return null;
            }
            return new JobUnitOfWork(_workItemRepository, workItemId.Value);
        }

        public void Dispose()
        {
            var repository = _workItemRepository;
            if (repository != null)
            {
                repository.Dispose();
            }
        }
    }
}
