using Code.Database.Models;
using Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using WebTN.Areas.Admin.Controllers;

namespace Web.Models.EF
{
    public class FoodContext : DbContext
    {
        public FoodContext(DbContextOptions<FoodContext> options) : base(options) { }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Authorized> Authorizeds { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Core.Database.Models.Group> Groups { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
    }
}
