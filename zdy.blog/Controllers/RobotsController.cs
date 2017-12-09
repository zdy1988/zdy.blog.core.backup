using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Zdy.Blog.Services;
using Microsoft.Extensions.Configuration;
using Zdy.Blog.Data.Models;
using System.Text;

namespace Zdy.Blog.Controllers
{
    public class RobotsController : Controller
    {
        private readonly IRepository _repository;
        private readonly IConfiguration _config;

        public RobotsController(IRepository repository,
            IConfiguration config)
        {
            _repository = repository;
            _config = config;
        }

        [Route("/robots.txt")]
        public string RobotsTxt()
        {
            string host = Request.Scheme + "://" + Request.Host;
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow:");
            sb.AppendLine($"sitemap: {host}/sitemap.xml");

            return sb.ToString();
        }

        [Route("/sitemap.xml")]
        public async Task SitemapXml()
        {
            string host = Request.Scheme + "://" + Request.Host;

            Response.ContentType = "application/xml";

            using (var xml = XmlWriter.Create(Response.Body, new XmlWriterSettings { Indent = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                var posts = await _repository.FindAsync<Post>(t => t.IsPublished);

                foreach (Post post in posts)
                {
                    var lastMod = new[] { post.PubDate, post.LastModified };

                    xml.WriteStartElement("url");
                    xml.WriteElementString("loc", $"{host}/post/{post.Slug}");
                    xml.WriteElementString("lastmod", lastMod.Max().ToString("yyyy-MM-ddThh:mmzzz"));
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }
        }
    }
}