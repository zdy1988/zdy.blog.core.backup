using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zdy.Blog.Filters
{
    public class UseCDNFilter : ActionFilterAttribute
    {
        private readonly IConfiguration _config;

        public UseCDNFilter(IConfiguration config)
        {
            _config = config;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ViewResult)
            {
                var result = context.Result as ViewResult;
                bool isUseCDN = _config.GetValue<bool>("cdn:isUseCDN");
                result.ViewData["CDN"] = isUseCDN ? _config.GetValue<string>("cdn:url") : "";
                result.ViewData["GalleryThumbnailSettings"] = isUseCDN ? "?" + _config.GetValue<string>("cdn:galleryThumbnailSettings") : "";
            }

            base.OnResultExecuting(context);
        }
    }
}
