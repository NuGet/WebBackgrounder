using System.Web.Hosting;

namespace WebBackgrounder {
    public class AspNetTaskHost : ITaskHost, IRegisteredObject {
        public AspNetTaskHost() {
            HostingEnvironment.RegisterObject(this);
        }

        public bool ShuttingDown {
            get;
            private set;
        }

        public void Stop(bool immediate) {
            lock (this) {
                ShuttingDown = true;
            }
        }
    }
}
