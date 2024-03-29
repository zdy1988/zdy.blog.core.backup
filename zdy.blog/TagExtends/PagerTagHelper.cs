﻿using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zdy.Blog.TagExtends
{
    public class PagerOption
    {
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public string RouteUrl { get; set; }
        public int PagerCount { get; set; } = 4;
    }

    public class PagerTagHelper : TagHelper
    {

        public PagerOption PagerOption { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.TagName = "div";

            if (PagerOption.PageSize <= 0) { PagerOption.PageSize = 15; }
            if (PagerOption.PageIndex <= 0) { PagerOption.PageIndex = 1; }
            if (PagerOption.TotalCount <= 0) { return; }

            // 计算分页
            var totalPage = PagerOption.TotalCount / PagerOption.PageSize + (PagerOption.TotalCount % PagerOption.PageSize > 0 ? 1 : 0);
            if (totalPage <= 0) { return; }

            var pageNumbers = new ArrayList();
            int start = 1;
            int end = PagerOption.PagerCount;
            bool isShowStart = false;
            bool isShowEnd = false;
            if (PagerOption.PageIndex >= PagerOption.PagerCount)
            {
                start = PagerOption.PageIndex - PagerOption.PagerCount / 2;
                isShowStart = true;
            }
            else
            {
                isShowStart = false;
            };
            end = start + PagerOption.PagerCount - 1;
            if (end > totalPage)
            {
                end = totalPage;
                isShowEnd = false;
            }
            else
            {
                isShowEnd = true;
            };
            for (var i = start; i <= end; i++)
            {
                pageNumbers.Add(i);
            };

            //当前路由地址
            if (string.IsNullOrEmpty(PagerOption.RouteUrl))
            {

                //PagerOption.RouteUrl = helper.ViewContext.HttpContext.Request.RawUrl;
                if (!string.IsNullOrEmpty(PagerOption.RouteUrl))
                {

                    var lastIndex = PagerOption.RouteUrl.LastIndexOf("/");
                    PagerOption.RouteUrl = PagerOption.RouteUrl.Substring(0, lastIndex);
                }
            }
            PagerOption.RouteUrl = PagerOption.RouteUrl.TrimEnd('/');

            //构造分页样式
            var pagerBuilder = new StringBuilder(string.Empty);
            pagerBuilder.Append("<nav class=\"pagination\">");

            if (isShowStart)
            {
                pagerBuilder.AppendFormat("<a class=\"page-number\" href=\"{0}/{1}\" aria-label=\"Previous\"><i class=\"icon-arrow-left\"></i></a>",
                   PagerOption.RouteUrl,
                   PagerOption.PageIndex - 1 <= 0 ? 1 : PagerOption.PageIndex - 1);
            }

            foreach (var i in pageNumbers)
            {
                if (Convert.ToInt32(i) == PagerOption.PageIndex)
                {
                    pagerBuilder.AppendFormat("<span class=\"page-number current\">{0}</span>",
                    i,
                    PagerOption.RouteUrl);
                }
                else
                {
                    pagerBuilder.AppendFormat("<a class=\"page-number\" href=\"{1}/{0}\">{0}</a>",
                    i,
                    PagerOption.RouteUrl);
                }
            }

            if (isShowEnd)
            {
                pagerBuilder.AppendFormat("<a class=\"page-number\" href=\"{0}/{1}\" aria-label=\"Next\"><i class=\"icon-arrow-right\"></i></a>",
                  PagerOption.RouteUrl,
                  PagerOption.PageIndex + 1 > totalPage ? PagerOption.PageIndex : PagerOption.PageIndex + 1);
            }

            pagerBuilder.Append("</nav>");

            output.Content.SetHtmlContent(pagerBuilder.ToString());
        }

    }
}
