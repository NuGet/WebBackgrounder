using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.DemoWeb.Controllers
{ 
    public class JobWorkersController : Controller
    {
        private JobsContext db = new JobsContext();

        //
        // GET: /JobWorkers/

        public ViewResult Index()
        {
            return View(db.JobWorkers.ToList());
        }

        //
        // GET: /JobWorkers/Details/5

        public ViewResult Details(int id)
        {
            JobWorker jobworker = db.JobWorkers.Find(id);
            return View(jobworker);
        }

        //
        // GET: /JobWorkers/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /JobWorkers/Create

        [HttpPost]
        public ActionResult Create(JobWorker jobworker)
        {
            if (ModelState.IsValid)
            {
                db.JobWorkers.Add(jobworker);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(jobworker);
        }
        
        //
        // GET: /JobWorkers/Edit/5
 
        public ActionResult Edit(int id)
        {
            JobWorker jobworker = db.JobWorkers.Find(id);
            return View(jobworker);
        }

        //
        // POST: /JobWorkers/Edit/5

        [HttpPost]
        public ActionResult Edit(JobWorker jobworker)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jobworker).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jobworker);
        }

        //
        // GET: /JobWorkers/Delete/5
 
        public ActionResult Delete(int id)
        {
            JobWorker jobworker = db.JobWorkers.Find(id);
            return View(jobworker);
        }

        //
        // POST: /JobWorkers/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            JobWorker jobworker = db.JobWorkers.Find(id);
            db.JobWorkers.Remove(jobworker);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}