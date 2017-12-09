using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zdy.Blog.Data.Models
{
    public class Category
    {
        public Guid ID { get; set; }
        public Guid SourceID { get; set; }
        public string Text { get; set; }
    }
}
