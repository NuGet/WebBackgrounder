
namespace WebBackgrounder {
    /// <summary>
    /// Represents the environment that is hosting the task manager. 
    /// Typically a web application such as ASP.NET.
    /// </summary>
    public interface ITaskHost {
        bool ShuttingDown { get; }
    }
}
