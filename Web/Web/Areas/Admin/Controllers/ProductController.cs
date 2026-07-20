using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Web.Data; // Đảm bảo namespace này trỏ đúng tới nơi chứa AppDbContext

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        // 1. Hàm trả về giao diện trang Quản lý sản phẩm
        // GET: Admin/Product
        public IActionResult Index()
        {
            return View();
        }

        // 2. Hàm API trả về danh sách sản phẩm dạng JSON cho AJAX
        // GET: Admin/Product/GetAll
        [HttpPost]
        public IActionResult Create([FromBody] Product model)
        {
            if (model != null)
            {
                _db.Products.Add(model);
                _db.SaveChanges();
                return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
        }
    }
}