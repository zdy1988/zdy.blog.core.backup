using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zdy.Blog.Models;
using Zdy.Blog.Data.Models;
using Zdy.Blog.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Zdy.Blog.Services;
using Microsoft.Extensions.Configuration;
using Zdy.Blog.TagExtends;
using Microsoft.EntityFrameworkCore;

namespace Zdy.Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRepository _repository;
        private readonly IUserServices _userServices;
        private readonly IConfiguration _config;

        public HomeController(IHostingEnvironment hostingEnvironment,
            IRepository repository,
            IUserServices userServices,
            IConfiguration config)
        {
            _hostingEnvironment = hostingEnvironment;
            _repository = repository;
            _userServices = userServices;
            _config = config;
        }

        [Route("/{pageIndex:int?}")]
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            var posts = await _repository.FindAsync<Post, DateTime>(t => t.IsPublished, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = "/"
            };

            ViewBag.PagerOption = pageOption;

            return View(posts);
        }

        [Route("/post/{slug?}")]
        public async Task<IActionResult> Post(string slug)
        {
            var post = await _repository.GetAsync<Post>(t => t.Slug == slug && t.IsPublished);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [Route("/archives/{pageIndex:int?}")]
        public async Task<IActionResult> Archives(int pageIndex = 1)
        {
            var posts = await _repository.FindAsync<Post, DateTime>(t => t.IsPublished, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = "/archives"
            };

            ViewBag.PagerOption = pageOption;

            return View(posts);
        }

        [Route("categories")]
        public async Task<IActionResult> Categories()
        {
            var query = _repository
                        .All<Category>()
                        .GroupBy(t => new { t.Text })
                        .Select(t2 => new KeyValuePair<string, int>(t2.Key.Text, t2.Count()));

            var data = await query.ToListAsync();

            return View(data);
        }

        [Route("search/{category}/{pageIndex:int?}")]
        public async Task<IActionResult> Search(string category, int pageIndex = 1)
        {
            var query = from posts in _repository.All<Post>()
                        join categories in _repository.All<Category>()
                        on posts.ID equals categories.SourceID
                        where categories.Text == category
                        select posts;

            var count = await query.GroupBy(t => t.ID).CountAsync();

            var data = await query.Distinct().OrderByDescending(t=>t.PubDate).Skip((pageIndex - 1) * int.Parse(_config["blog:pageSize"])).Take(int.Parse(_config["blog:pageSize"])).ToListAsync();

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = $"/search/{category}"
            };

            ViewBag.PagerOption = pageOption;

            return View("Archives", data);
        }

        [Route("photos/{pageIndex:int?}")]
        public async Task<IActionResult> Photos(int pageIndex = 1)
        {
            var photos = await _repository.FindAsync<Gallery, DateTime>(t => t.IsPublished, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = "/photos"
            };

            ViewBag.PagerOption = pageOption;

            return View(photos);
        }

        [Route("search2/{category}/{pageIndex:int?}")]
        public async Task<IActionResult> Search2(string category, int pageIndex = 1)
        {
            var query = from galleries in _repository.All<Gallery>()
                        join categories in _repository.All<Category>()
                        on galleries.ID equals categories.SourceID
                        where categories.Text == category
                        select galleries;

            var count = await query.GroupBy(t => t.ID).CountAsync();

            var data = await query.Distinct().OrderByDescending(t => t.PubDate).Skip((pageIndex - 1) * int.Parse(_config["blog:pageSize"])).Take(int.Parse(_config["blog:pageSize"])).ToListAsync();

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = $"/search/{category}"
            };

            ViewBag.PagerOption = pageOption;

            return View("Photos", data);
        }

        [Route("/photo/{slug?}")]
        public async Task<IActionResult> Photo(string slug)
        {
            var gallery = await _repository.GetAsync<Gallery>(t => t.Slug == slug && t.IsPublished);

            if (gallery == null)
            {
                return NotFound();
            }

            return View(gallery);
        }

        [Route("iland")]
        public async Task<IActionResult> ILand()
        {
            var post = await _repository.GetAsync<Post>(t => t.Slug == "ILand" && t.IsPublished);

            if (post == null)
            {
                return NotFound();
            }

            return View("Post", post);
        }

        [Route("about")]
        public async Task<IActionResult> About()
        {
            var post = await _repository.GetAsync<Post>(t => t.Slug == "About" && t.IsPublished);

            if (post == null)
            {
                return NotFound();
            }

            return View("Post", post);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/comment")]
        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Content = comment.Content.Trim();
                comment.Author = comment.Author.Trim();
                comment.Email = comment.Email.Trim();
                comment.PubDate = DateTime.Now;
                comment.IsAdmin = User.Identity.IsAuthenticated;
                comment.IsApproved = !Boolean.Parse(_config["blog:IsCommentApproved"]);

                await _repository.InsertAsync<Comment>(comment);
                await _repository.SaveChangesAsync();
            }

            return View("_CommentsItem", comment);
        }

        [Route("/comments/{sourceId}/{pageIndex:int?}")]
        [HttpPost]
        public async Task<IActionResult> GetComments(Guid sourceId, int pageIndex = 1)
        {
            var comments = await _repository.FindAsync<Comment, DateTime>(t => t.SourceID == sourceId && t.IsApproved == true, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            return View("_CommentsList", new Tuple<IEnumerable<Comment>, int, int>(comments, count, pageIndex));
        }

        [HttpPost]
        [Route("UploadFiles")]
        public async Task<IActionResult> UploadFiles()
        {
            var type = Request.Form["type"];
            string relative = _hostingEnvironment.WebRootPath + $"/upload/{type}/";
            if (!Directory.Exists(relative))
            {
                Directory.CreateDirectory(relative);
            }
            List<string> fileResults = new List<string>();
            long size = 0;
            var files = Request.Form.Files;
            foreach (var file in files)
            {
                string extension = Path.GetExtension(file.FileName).ToLower();
                if (extension == ".jpg" || extension == ".gif" || extension == ".jpeg" || extension == ".png")
                {
                    string fileId = Guid.NewGuid().ToString();
                    string fileName = fileId + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(relative, fileName);
                    size += file.Length;
                    await Task.Run(() =>
                    {
                        using (Image<Rgba32> image = Image.Load(file.OpenReadStream()))
                        {
                            if (file.Length > 200 * 1024)
                            {
                                int imageWidth = string.IsNullOrEmpty(Request.Form["imageWidth"]) ? 1080 : int.Parse(Request.Form["imageWidth"]);
                                var imageHeight = image.Height * imageWidth / image.Width;
                                image.Mutate(x => x.Resize(imageWidth, imageHeight));
                            }
                            image.Save(filePath);
                        }
                    });
                    fileResults.Add(filePath.Replace(_hostingEnvironment.WebRootPath, ""));

                    //gallery
                    if (type.ToString().IndexOf("gallery") != -1)
                    {
                        Guid galleryID = new Guid(type.ToString().Substring(8));
                        await _repository.InsertAsync<Photo>(new Photo
                        {
                            ID = new Guid(fileId),
                            SourceID = galleryID,
                            Excerpt = file.FileName,
                            FileName = fileName,
                            Size = file.Length,
                            PubDate = DateTime.Now
                        });
                        await _repository.SaveChangesAsync();
                    }
                }
            }
            string message = $"{files.Count} file(s) / {size} bytes uploaded successfully!";
            return Json(new
            {
                message,
                fileResults
            });
        }
    }
}
