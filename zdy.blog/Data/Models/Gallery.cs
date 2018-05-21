using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zdy.Blog.Data.Models
{
    public class Gallery
    {
        public Gallery()
        {
            PubDate = DateTime.Now;
            IsPublished = true;
        }

        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Excerpt { get; set; }
        public DateTime PubDate { get; set; }
        public bool IsPublished { get; set; }
        public int CheckNumber { get; set; } = 0;
        public string Categories { get; set; }
    }
}
