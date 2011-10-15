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
        readonly Func<string, IWorkItemRepository> _repositoryThunk;

        public WebFarmJobCoordinator(Func<string, IWorkItemRepository> repositoryThunk)
        {
            _repositoryThunk = repositoryThunk;
        }

        public Task PerformWork(IJob job)
        {
            // We need a new instance every time we perform work.
            var repository = _repositoryThunk(job.Name);

            var unitOfWork = ReserveWork(repository, WorkerId);
            if (unitOfWork == null)
            {
                return null;
            }

            var task = job.Execute();
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

        public JobUnitOfWork ReserveWork(IWorkItemRepository repository, string workerId)
        {
            long? workItemId = null;

            // We do a double check here because this is the first query we run and 
            // a database can't be created inside a transaction scope.
            if (repository.AnyActiveWorker)
            {
                return null;
            }

            repository.RunInTransaction(() =>
                {
                    if (repository.AnyActiveWorker)
                    {
                        workItemId = null;
                        return;
                    }
                    workItemId = repository.CreateWorkItem(workerId);
                }
            );

            if (workItemId == null)
            {
                return null;
            }
            return new JobUnitOfWork(repository, workItemId.Value);
        }

    }
}
