using Microsoft.EntityFrameworkCore;
using Web.Data; // Thêm namespace này
using Web.Models.EF;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm Services
builder.Services.AddControllersWithViews();

// Đăng ký FoodContext (nếu vẫn xài)
builder.Services.AddDbContext<FoodContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodDb")));

// 👉 ĐÃ THÊM: Đăng ký AppDbContext cho ProductController
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodDb")));
// Lưu ý: Nếu chuỗi kết nối trong appsettings.json là "FoodDb" thì giữ nguyên "FoodDb". 
// Còn nếu bạn đặt tên khác (như "DefaultConnection") thì sửa tên chuỗi cho khớp nhé!

var app = builder.Build();

// 2. Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 3. Khai báo Route cho Area (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// 4. Khai báo Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();