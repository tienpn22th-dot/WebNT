using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly FoodContext _context;

        public HomeController(FoodContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Thống kê nhanh ra Thẻ (Cards)
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            // Tính tổng tiền các đơn hàng (TotalAmount trong Order là kiểu double)
            ViewBag.TotalRevenue = await _context.Orders
                .SumAsync(o => (double?)o.TotalAmount) ?? 0;

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalUsers = await _context.Members.CountAsync();

            return View();
        }

        #region API LẤY DỮ LIỆU CHO BIỂU ĐỒ (Chart.js)

        [HttpGet]
        public async Task<IActionResult> GetRevenueChartData()
        {
            // Lấy 7 ngày gần nhất
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            // Nhóm đơn hàng theo ngày tạo (Sử dụng CreatedOn)
            var orderData = await _context.Orders
                .Where(o => o.CreatedOn >= last7Days.First())
                .GroupBy(o => o.CreatedOn.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            var labels = last7Days.Select(d => d.ToString("dd/MM")).ToArray();
            var totals = last7Days.Select(d => orderData.FirstOrDefault(o => o.Date == d)?.Total ?? 0).ToArray();

            return Json(new { labels, totals });
        }

        #endregion
    }
}