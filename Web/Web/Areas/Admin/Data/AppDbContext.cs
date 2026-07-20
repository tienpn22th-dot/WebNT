using Microsoft.EntityFrameworkCore;
using Core.Database.Models; // Hoặc namespace chứa các Model của bạn

namespace Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Authorized> Authorizeds { get; set; }
    }
}