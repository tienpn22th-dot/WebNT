using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Controllers
{
    public class ProductController : Controller
    {
        private readonly FoodContext _context;

        public ProductController(FoodContext context)
        {
            _context = context;
        }

        // GET: /Product hoặc /Product/Index?categoryId=1&keyword=tra&page=1
        public async Task<IActionResult> Index(int? categoryId, string? keyword, int page = 1)
        {
            int pageSize = 9; // Số sản phẩm trên 1 trang

            // Query kết nối bảng SanPham và Include bảng DanhMuc
            var query = _context.SanPhams
                .Include(p => p.DanhMuc)
                .AsQueryable();

            // 1. Lọc theo Danh mục (MaDanhMuc)
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.MaDanhMuc == categoryId.Value);
                ViewBag.CurrentCategoryId = categoryId.Value;
            }

            // 2. Lọc theo Từ khóa tìm kiếm (TenSp)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.TenSp.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            // Tính tổng số trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Sắp xếp theo MaSp giảm dần và phân trang
            var products = await query
                .OrderByDescending(p => p.MaSp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách danh mục hiển thị Sidebar
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }

        // GET: /Product/Detail/6
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .FirstOrDefaultAsync(p => p.MaSp == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy 4 sản phẩm liên quan cùng danh mục
            ViewBag.RelatedProducts = await _context.SanPhams
                .Where(p => p.MaDanhMuc == product.MaDanhMuc && p.MaSp != id)
                .Take(4)
                .ToListAsync();

            return View(product);
        }
    }
}