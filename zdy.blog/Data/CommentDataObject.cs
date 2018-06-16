using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zdy.Blog.Data.Models;

namespace Zdy.Blog.Data
{
    public class CommentDataObject: Comment
    {
        public string Code { get; set; }
    }
}
