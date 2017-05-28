using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NewsBlog.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;

namespace NewsBlog.Controllers
{
    public class ArticlesController : Controller
    {
        private ApplicationDbContext db;

        public ArticlesController()
        {
           db = new ApplicationDbContext();
        }

        public ActionResult Index(int? page)
        {
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            ViewBag.User = User.Identity.Name;

            var articles = db.Articles.ToList().OrderByDescending(p => p.ID).ToPagedList(pageNumber, pageSize);
            return View("Index", articles);
        }

        [AllowAnonymous]
        public FileContentResult GetCoverImage(int id)
        {
            var cover = db.Articles.FirstOrDefault(p => p.ID == id);
            if (cover.Cover != null && cover.CoverType != null)
            {
                return File(cover.Cover, cover.CoverType);
            }

            else
            {
                return null;
            }
        }

        [AllowAnonymous]
        public FileContentResult GetImageTop(int id)
        {

            var result = db.Articles.First(a => a.ID == id);


            if (result != null)
            {
                return File(result.Cover, result.CoverType);
            }

            else
            {
                return null;
            }
        }

        private List<Categories> GetOptions()
        {
            return db.Categories.ToList();
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        public ActionResult Create()
        {
            Article article = new Article();
            ViewBag.Categories = GetOptions();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Content, DateCreate, Cover, CoverType, Description, CategoriesID")] Article article, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                        article.Cover = array;
                        article.CoverType = file.ContentType;
                    }
                }
                article.DateCreate = DateTime.Now;
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                var currentUser = manager.FindById(User.Identity.GetUserId());
                article.UserID = User.Identity.GetUserId();
                db.Articles.Add(article);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(article);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);
            ViewBag.Categories = GetOptions();
            if (article == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoriesID = new SelectList(db.Categories, "ID", "Name", article.CategoriesID);
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID, Name,Content, CategoriesID, Description, Cover, CoverType")] Article article, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                        article.Cover = array;
                        article.CoverType = file.ContentType;
                    }
                }
                db.Entry(article).State = EntityState.Modified;

                if (file != null)
                {
                    db.Entry(article).Property(r => r.Cover).IsModified = true;
                    db.Entry(article).Property(r => r.CoverType).IsModified = true;
                }
                else
                {
                    db.Entry(article).Property(r => r.Cover).IsModified = false;
                    db.Entry(article).Property(r => r.CoverType).IsModified = false;
                }
                db.Entry(article).Property(r => r.UserID).IsModified = false;
                db.Entry(article).Property(r => r.DateCreate).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(article);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Article article = db.Articles.Find(id);
                db.Articles.Remove(article);
                db.SaveChanges();
                return Json(new { Success = true });
            }
            catch
            {//TODO: Log error				
                return Json(new { Success = false });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
