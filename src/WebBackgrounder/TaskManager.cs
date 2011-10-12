using System;
using System.Collections.Generic;
using System.Threading;

namespace WebBackgrounder {
    public class TaskManager {
        readonly ITaskHost _host;
        readonly Timer _timer;
        readonly IJobCoordinator _coordinator;
        readonly Guid _workerId = Guid.NewGuid();
        readonly IEnumerable<ITask> _tasks;

        public TaskManager(IEnumerable<ITask> tasks, ITaskHost host, IJobCoordinator coordinator) {
            _tasks = tasks;
            _host = host;
            _coordinator = coordinator;
            _timer = new Timer(OnTimerElapsed);
        }

        void OnTimerElapsed(object sender) {
            _timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop, in the name of love.
            try {
                foreach (var task in _tasks) {
                    RunTask(task);
                }
            }
            finally {
                _timer.Change(10000, Timeout.Infinite); // Start up again.
            }
        }

        public void RunTask(ITask task) {
            lock (_host) {
                if (!_host.ShuttingDown && _coordinator.CanDoWork(task.JobName, _workerId)) {
                    using (_coordinator.StartWork(task.JobName, _workerId)) {
                        task.Execute();
                    }
                }
            }
        }
    }
}
