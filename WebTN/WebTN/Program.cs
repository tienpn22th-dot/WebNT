using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình DbContext kết nối SQL Server qua FoodDb
builder.Services.AddDbContext<FoodContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodDb")));

// --------------------------------------------------
// 1. CẤU HÌNH DỊCH VỤ SESSION
// --------------------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --------------------------------------------------
// 2. KÍCH HOẠT MIDDLEWARE SESSION (Bắt buộc đặt trước UseAuthorization)
// --------------------------------------------------
app.UseSession();

app.UseAuthorization();

// Route cho Admin Area
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}"
);

// Route mặc định cho Client
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();