using System.Linq;
using System.Web.Mvc;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.DemoWeb.Controllers {
    public class HomeController : Controller {
        private JobsContext db = new JobsContext();

        public ActionResult Index() {
            var workers = (from w in db.JobWorkers
                           orderby w.Started descending
                           select w).Take(30);
            return View(workers);
        }
    }
}
