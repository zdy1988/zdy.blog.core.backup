using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zdy.Blog.Data.Models;
using Zdy.Blog.Services;
using Microsoft.Extensions.Configuration;
using Zdy.Blog.TagExtends;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Zdy.Blog.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Zdy.Blog.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRepository _repository;
        private readonly IUserServices _userServices;
        private readonly IConfiguration _config;

        public AdminController(IHostingEnvironment hostingEnvironment,
            IRepository repository,
            IUserServices userServices,
            IConfiguration config)
        {
            _hostingEnvironment = hostingEnvironment;
            _repository = repository;
            _userServices = userServices;
            _config = config;
        }

        [AllowAnonymous]
        public IActionResult Index(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string returnUrl, LoginViewModel model)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid && _userServices.ValidateUser(model.UserName, model.Password))
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));

                var principle = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                    IsPersistent = model.RememberMe,
                    AllowRefresh = false
                };
                await HttpContext.SignInAsync(principle, properties);

                return LocalRedirect(returnUrl ?? "/admin");
            }

            ModelState.AddModelError(string.Empty, "username or password is invalid.");
            return View("Index", model);
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/admin");
        }

        public async Task<IActionResult> GalleryEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(new Gallery());
            }

            var gallery = await _repository.GetAsync<Gallery>(t => t.ID == new Guid(id));

            if (gallery != null)
            {
                return View(gallery);
            }

            return NotFound();
        }

        [Route("/Admin/GalleryManage/{pageIndex?}")]
        public async Task<IActionResult> GalleryManage(int pageIndex = 1)
        {
            var galleries = await _repository.FindAsync<Gallery, DateTime>(t => true, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = "/Admin/GalleryManage"
            };

            ViewBag.PagerOption = pageOption;

            return View(galleries);
        }

        [HttpPost]
        public async Task<IActionResult> GalleryUpdate(Gallery gallery)
        {
            if (!ModelState.IsValid)
            {
                RedirectToAction("GalleryEdit", "Admin");
            }

            var existSlug = await _repository.GetAsync<Gallery>(t => t.ID != gallery.ID && t.Slug == gallery.Slug);
            if (existSlug != null)
            {
                return Json(new { IsSuccess = false, Message = "the slug repeated!" });
            }

            var exist = await _repository.GetAsync<Gallery>(t => t.ID == gallery.ID) ?? gallery;

            if (exist.ID == Guid.Empty)
            {
                await _repository.InsertAsync<Gallery>(exist);
            }
            else
            {
                exist.Title = gallery.Title;
                exist.Slug = gallery.Slug;
                exist.PubDate = gallery.PubDate;
                exist.Excerpt = gallery.Excerpt;
                exist.IsPublished = gallery.IsPublished;
                exist.Categories = gallery.Categories;
                _repository.Update<Gallery>(exist);
            }

            await _repository.SaveChangesAsync();

            _repository.Delete<Category>(t => t.SourceID == exist.ID);

            if (!string.IsNullOrEmpty(gallery.Categories))
            {
                string[] categories = gallery.Categories.Split(",", StringSplitOptions.RemoveEmptyEntries);

                foreach (var category in categories)
                {
                    await _repository.InsertAsync<Category>(
                        new Category
                        {
                            SourceID = exist.ID,
                            Text = category
                        });
                }

                await _repository.SaveChangesAsync();
            }

            return Json(new { IsSuccess = true, Message = "post success!", Data = exist });
        }

        public async Task<IActionResult> GalleryDelete(string galleryId)
        {
            var gallery = _repository.Get<Gallery>(t => t.ID == new Guid(galleryId));

            if (gallery == null)
            {
                Json(new { IsSuccess = false, Message = "not found!" });
            }

            _repository.Delete<Gallery>(gallery);

            await _repository.SaveChangesAsync();

            var dirPath = _hostingEnvironment.WebRootPath + $"/upload/gallery/{gallery.ID}";

            if (System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.Delete(dirPath, true);
            }

            return Json(new { IsSuccess = true });
        }

        [HttpPost]
        public async Task<IActionResult> PhotoUpdate(Photo photo)
        {
            if (!ModelState.IsValid)
            {
                RedirectToAction("GalleryEdit", "Admin");
            }

            var exist = await _repository.GetAsync<Photo>(t => t.ID == photo.ID);

            if (exist != null)
            {
                exist.Title = photo.Title;
                exist.Sort = photo.Sort;

                _repository.Update<Photo>(exist);
                await _repository.SaveChangesAsync();

                return Json(new { IsSuccess = true, Message = "post success!", Data = exist });
            }

            return NotFound();
        }

        public async Task<IActionResult> PhotoDelete(string photoId)
        {
            var photo = _repository.Get<Photo>(t => t.ID == new Guid(photoId));

            if (photo == null)
            {     
                Json(new { IsSuccess = false, Message = "not found!" });
            }

            _repository.Delete<Photo>(photo);

            await _repository.SaveChangesAsync();

            var filePath = _hostingEnvironment.WebRootPath + $"/upload/gallery/{photo.SourceID}/{photo.FileName}";

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return Json(new { IsSuccess = true });
        }

        public async Task<IActionResult> PostEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(new Post());
            }

            var post = await _repository.GetAsync<Post>(t => t.ID == new Guid(id));

            if (post != null)
            {
                return View(post);
            }

            return NotFound();
        }

        [Route("/Admin/PostManage/{pageIndex?}")]
        public async Task<IActionResult> PostManage(int pageIndex = 1)
        {
            var posts = await _repository.FindAsync<Post, DateTime>(t => true, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = "/Admin/PostManage"
            };

            ViewBag.PagerOption = pageOption;

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdate(Post post)
        {
            if (!ModelState.IsValid)
            {
                RedirectToAction("PostEdit", "Admin");
            }

            var existSlug = await _repository.GetAsync<Post>(t => t.ID != post.ID && t.Slug == post.Slug);
            if (existSlug != null)
            {
                return Json(new { IsSuccess = false, Message = "the slug repeated!" });
            }

            var exist = await _repository.GetAsync<Post>(t => t.ID == post.ID) ?? post;

            exist.Author = _config["blog:Author"];

            if (exist.ID == Guid.Empty)
            {
                await _repository.InsertAsync<Post>(exist);
            }
            else
            {
                exist.Title = post.Title;
                exist.Slug = post.Slug;
                exist.PubDate = post.PubDate;
                exist.Excerpt = post.Excerpt;
                exist.Content = post.Content;
                exist.LastModified = DateTime.Now;
                exist.IsPublished = post.IsPublished;
                exist.Categories = post.Categories;
                _repository.Update<Post>(exist);
            }

            await _repository.SaveChangesAsync();

            _repository.Delete<Category>(t => t.SourceID == exist.ID);

            if (!string.IsNullOrEmpty(post.Categories))
            {
                string[] categories = post.Categories.Split(",", StringSplitOptions.RemoveEmptyEntries);

                foreach (var category in categories)
                {
                    await _repository.InsertAsync<Category>(
                        new Category
                        {
                            SourceID = exist.ID,
                            Text = category
                        });
                }

                await _repository.SaveChangesAsync();
            }

            return Json(new { IsSuccess = true, Message = "post success!", Data = exist });
        }

        public async Task<IActionResult> PostDelete(string postId)
        {
            var post = await _repository.GetAsync<Post>(t => t.ID == new Guid(postId));

            if (post == null)
            {
                Json(new { IsSuccess = false, Message = "not found!" });
            }

            _repository.Delete<Post>(post);

            await _repository.SaveChangesAsync();

            return Json(new { IsSuccess = true });
        }

        [Route("/Admin/CommentManage/{pageIndex?}")]
        public async Task<IActionResult> CommentManage(int pageIndex = 1)
        {
            int pageSize = 10;

            var comments = await _repository.FindAsync<Comment, DateTime>(t => true, t => t.PubDate, pageSize, pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = count,
                RouteUrl = "/Admin/CommentManage"
            };

            ViewBag.PagerOption = pageOption;

            return View(comments);
        }

        public async Task<IActionResult> CommentDelete(string commentId)
        {
            var comment = await _repository.GetAsync<Comment>(t => t.ID == new Guid(commentId));

            if (comment == null)
            {
                Json(new { IsSuccess = false, Message = "not found!" });
            }

            _repository.Delete<Comment>(comment);

            await _repository.SaveChangesAsync();

            return Json(new { IsSuccess = true });
        }

        public async Task<IActionResult> CommentApproved(string commentId)
        {
            var comment = await _repository.GetAsync<Comment>(t => t.ID == new Guid(commentId));

            if (comment == null)
            {
                Json(new { IsSuccess = false, Message = "not found!" });
            }

            comment.IsApproved = true;

            _repository.Update<Comment>(comment);

           await _repository.SaveChangesAsync();

            return Json(new { IsSuccess = true });
        }
    }
}