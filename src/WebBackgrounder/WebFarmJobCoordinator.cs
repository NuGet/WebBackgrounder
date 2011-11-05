using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator
    {
        readonly static string WorkerId = Guid.NewGuid().ToString();
        readonly IWorkItemRepository _workItemRepository;

        public WebFarmJobCoordinator(IWorkItemRepository workItemRepository)
        {
            if (workItemRepository == null)
            {
                throw new ArgumentNullException("workItemRepository");
            }
            _workItemRepository = workItemRepository;
        }

        public Task GetWork(IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            // We need a new instance every time we perform work.
            var unitOfWork = ReserveWork(WorkerId, job.Name);
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

        public JobUnitOfWork ReserveWork(string workerId, string jobName)
        {
            long? workItemId = null;

            // We do a double check here because this is the first query we run and 
            // a database can't be created inside a transaction scope.
            if (_workItemRepository.GetActiveWorker(jobName) != null)
            {
                return null;
            }

            _workItemRepository.RunInTransaction(() =>
                {
                    if (_workItemRepository.GetActiveWorker(jobName) != null)
                    {
                        workItemId = null;
                        return;
                    }
                    workItemId = _workItemRepository.CreateWorkItem(workerId, jobName);
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
