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
using SixLabors.ImageSharp;
using Zdy.Blog.Services;
using Microsoft.Extensions.Configuration;
using Zdy.Blog.TagExtends;
using Microsoft.EntityFrameworkCore;
using System.DrawingCore;
using System.DrawingCore.Imaging;

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

            post.CheckNumber += 1;
            _repository.Update<Post>(post);
            await _repository.SaveChangesAsync();

            return View(post);
        }

        [Route("/archives/{pageIndex:int?}")]
        public async Task<IActionResult> Archives(int pageIndex = 1)
        {
            int pageSize = 20;

            var posts = await _repository.FindAsync<Post, DateTime>(t => t.IsPublished, t => t.PubDate, pageSize, pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
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
                        where categories.Text == category && posts.IsPublished
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

        [Route("gallery/{pageIndex:int?}")]
        public async Task<IActionResult> Gallery(int pageIndex = 1)
        {
            var photos = await _repository.FindAsync<Gallery, DateTime>(t => t.IsPublished, t => t.PubDate, int.Parse(_config["blog:pageSize"]), pageIndex, out int count);

            var pageOption = new PagerOption
            {
                PageIndex = pageIndex,
                PageSize = int.Parse(_config["blog:pageSize"]),
                TotalCount = count,
                RouteUrl = "/gallery"
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
                        where categories.Text == category && galleries.IsPublished
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

            gallery.CheckNumber += 1;
            _repository.Update<Gallery>(gallery);
            await _repository.SaveChangesAsync();

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

        [Route("music")]
        public IActionResult Music()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/comment")]
        [HttpPost]
        public async Task<IActionResult> AddComment(CommentDataObject comment)
        {
            if (String.IsNullOrEmpty(comment.Code) || comment.Code.ToLower() != HttpContext.Session.GetString("ValidateCode").ToLower())
            {
                return Content("Message:验证码有误");
            }

            if (ModelState.IsValid)
            {
                Comment newComment = new Comment();
                newComment.SourceID = comment.SourceID;
                newComment.Content = comment.Content.Trim();
                newComment.Author = comment.Author.Trim();
                newComment.Email = comment.Email.Trim();
                newComment.PubDate = DateTime.Now;
                newComment.Ip = this.HttpContext.Connection.RemoteIpAddress.ToString();
                newComment.UserAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
                newComment.IsAdmin = User.Identity.IsAuthenticated;
                newComment.IsApproved = !Boolean.Parse(_config["blog:IsCommentApproved"]);

                await _repository.InsertAsync<Comment>(newComment);
                await _repository.SaveChangesAsync();

                return View("_CommentsItem", newComment);
            }
            else
            {
                return Content("");
            }
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
                    //压缩图片
                    //await Task.Run(() =>
                    //{
                    //    using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()))
                    //    {
                    //        if (file.Length > 200 * 1024)
                    //        {
                    //            int imageWidth = string.IsNullOrEmpty(Request.Form["imageWidth"]) ? 1080 : int.Parse(Request.Form["imageWidth"]);
                    //            var imageHeight = image.Height * imageWidth / image.Width;
                    //            image.Mutate(x => x.Resize(imageWidth, imageHeight));
                    //        }
                    //        image.Save(filePath);
                    //    }
                    //});

                    using (var stream = new FileStream(filePath, FileMode.CreateNew))
                    {
                        await file.CopyToAsync(stream);
                    }

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

        [Route("ValidateCode")]
        public IActionResult ValidateCode()
        {
            int Width = 100;
            int Height = 36;

            string chkCode = string.Empty;

            //颜色列表，用于验证码、噪线、噪点
            Color[] color = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.DarkBlue };

            //字体列表，用于验证码
            string[] font = { "Times New Roman", "MS Mincho", "Book Antiqua", "Gungsuh", "PMingLiU", "Impact" };

            //验证码的字符集，去掉了一些容易混淆的字符
            char[] character ={ '2', '3', '4', '5', '6', '8', '9',
                                'A', 'B', 'C', 'D', 'E','F', 'G',
                                'H', 'J', 'K', 'L', 'M', 'N',
                                'P', 'R', 'S', 'T', 'W', 'X', 'Y' };

            Random rnd = new Random();

            //生成验证码字符串
            for (int i = 0; i < 4; i++)
            {
                chkCode += character[rnd.Next(character.Length)];
            }

            Bitmap bmp = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            //画噪线
            for (int i = 0; i < 2; i++)
            {
                int x1 = rnd.Next(Width);
                int y1 = rnd.Next(Height);
                int x2 = rnd.Next(Width);
                int y2 = rnd.Next(Height);
                Color clr = color[rnd.Next(color.Length)];
                g.DrawLine(new Pen(clr), x1, y1, x2, y2);
            }

            //画验证码字符串
            for (int i = 0; i < chkCode.Length; i++)
            {
                string fnt = font[rnd.Next(font.Length)];
                Font ft = new Font(fnt, 16, FontStyle.Bold);
                Color clr = color[rnd.Next(color.Length)];
                g.DrawString(
                    chkCode[i].ToString(),
                    ft,
                    new SolidBrush(clr),
                    (float)i * 20 + 20,
                    (float)6
                    );
            }

            //画噪点
            for (int i = 0; i < 10; i++)
            {
                int x = rnd.Next(bmp.Width);
                int y = rnd.Next(bmp.Height);
                Color clr = color[rnd.Next(color.Length)];
                bmp.SetPixel(x, y, clr);
            }
            //将验证码图片写入内存流，并将其以"image/Png" 格式输出
            MemoryStream ms = new MemoryStream();

            try
            {
                bmp.Save(ms, ImageFormat.Png);
                HttpContext.Session.SetString("ValidateCode", chkCode);
                return File(ms.ToArray(), @"image/png");
            }
            finally
            {
                //显式释放资源
                bmp.Dispose();
                g.Dispose();
            }
        }
    }
}
