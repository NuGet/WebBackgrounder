using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace WebBackgrounder
{
    public class JobHost : IJobHost, IRegisteredObject
    {
        readonly object _lock = new object();
        bool _shuttingDown;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public JobHost()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            if (immediate)
                _cts.Cancel();
            lock (_lock)
            {
                _shuttingDown = true;
            }
            HostingEnvironment.UnregisterObject(this);
        }

        public void DoWork(IJob work)
        {
            if (work == null)
            {
                throw new ArgumentNullException("work");
            }
            lock (_lock)
            {
                if (_shuttingDown || _cts.IsCancellationRequested)
                {
                    return;
                }

                var task = work.Execute(_cts.Token);

                if (task.Status == TaskStatus.Created)
                {
                    task.Start();
                }

                try
                {
                    // Wait() rethrows exceptions (if any) wrapped in an AggregateException.
                    task.Wait();
                }
                catch (AggregateException ae)
                {
                    if (ae.Flatten().InnerException as OperationCanceledException == null)
                    {
                        throw; // Rethrow if the real exception wasn't OperationCanceledException.
                    }
                }
            }
        }
    }
}
