using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zdy.Blog.Data.Models
{
    public class Photo
    {
        public Photo()
        {
            PubDate = DateTime.Now;
        }

        public Guid ID { get; set; }
        public Guid SourceID { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTime PubDate { get; set; }
    }
}
