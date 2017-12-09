using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zdy.Blog.TagExtends
{
    public class MarkdownOption
    {
        public string StringContent { get; set; }
        public bool HandleMore { get; set; } = false;
    }

    public class MarkdownTagHelper : TagHelper
    {
        public MarkdownOption MarkdownOption { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string content = MarkdownOption.StringContent;

            if (MarkdownOption.HandleMore)
            {
                content = content.HandleMore();
            }

            output.TagName = "div";

            var settings = CommonMark.CommonMarkSettings.Default.Clone();
            settings.RenderSoftLineBreaksAsLineBreaks = true;

            var outputHtml = CommonMark.CommonMarkConverter.Convert(content, settings);

            output.Content.SetHtmlContent(outputHtml);
        }
    }

    public static class ContentExtension
    {
        public static string HandleMore(this string content)
        {
            var moreIndex = content.IndexOf("<!-- more -->");
            if (moreIndex != -1)
            {
                content = content.Substring(0, moreIndex);
            }
            return content;
        }
    }
}
