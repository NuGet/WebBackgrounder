using System.Threading.Tasks;

namespace WebBackgrounder {
    public interface ITask {
        string JobName { get; }
        Task Execute();
    }
}
