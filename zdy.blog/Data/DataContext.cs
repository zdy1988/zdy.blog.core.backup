using Microsoft.EntityFrameworkCore;
using Zdy.Blog.Data.Models;

namespace Zdy.Blog.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Post> Post { set; get; }

        public DbSet<Photo> Photo { set; get; }

        public DbSet<Gallery> Gallery { set; get; }

        public DbSet<Comment> Comment { set; get; }

        public DbSet<Category> Category { set; get; }
    }
}
