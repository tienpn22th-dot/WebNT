using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace TeaHouse.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly FoodContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(FoodContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ViewModel nhận dữ liệu từ FormData (Upload file)
        public class ProductSaveModel
        {
            public string? Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public double? Price { get; set; }
            public string? CategoryId { get; set; }
            public string? Intro { get; set; }
            public string? Content { get; set; }
            public IFormFile? PictureFile { get; set; }
        }

        // 1. Lấy danh sách cho DataTables (Server-side)
        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.Products
                    .Include(p => p.Category)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(p => p.Title.Contains(searchValue) || (p.Category != null && p.Category.Name.Contains(searchValue)));
                }

                int recordsTotal = await query.CountAsync();

                var data = await query
                    .OrderBy(p => p.Title)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        id = p.Id,
                        title = p.Title,
                        picture = p.Picture ?? "/images/no-image.png",
                        price = p.Price ?? 0,
                        categoryName = p.Category != null ? p.Category.Name : "Chưa phân loại",
                        intro = p.Intro
                    })
                    .ToListAsync();

                return Json(new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new { draw = "1", recordsFiltered = 0, recordsTotal = 0, data = new List<object>(), error = ex.Message });
            }
        }

        // 2. Lấy thông tin 1 Sản phẩm để Sửa
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            return Json(new
            {
                id = p.Id,
                title = p.Title,
                price = p.Price,
                categoryId = p.CategoryId,
                intro = p.Intro,
                content = p.Content,
                picture = p.Picture
            });
        }

        // 3. Lưu (Thêm mới hoặc Cập nhật + Upload ảnh)
        [HttpPost]
        public async Task<IActionResult> Save([FromForm] ProductSaveModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Title))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên sản phẩm!" });
                }

                // Kiểm tra CategoryId hợp lệ (Xử lý trường hợp rỗng hoặc chọn "-- Chọn danh mục --")
                Guid? categoryGuid = null;
                if (!string.IsNullOrWhiteSpace(model.CategoryId) && Guid.TryParse(model.CategoryId, out Guid parsedCatId))
                {
                    categoryGuid = parsedCatId;
                }

                string? fileName = null;

                // Xử lý Upload Ảnh nếu người dùng chọn file mới
                if (model.PictureFile != null && model.PictureFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tên file rút gọn để tránh vượt độ dài cột SQL
                    string ext = Path.GetExtension(model.PictureFile.FileName);
                    string uniqueFileName = $"{Guid.NewGuid().ToString("N")[..8]}_{DateTime.Now.Ticks}{ext}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PictureFile.CopyToAsync(fileStream);
                    }

                    fileName = "/uploads/products/" + uniqueFileName;
                }

                if (string.IsNullOrEmpty(model.Id))
                {
                    // Thêm mới
                    var newProduct = new Product
                    {
                        Id = Guid.NewGuid(),
                        Title = model.Title.Trim(),
                        Price = model.Price ?? 0,
                        CategoryId = categoryGuid,
                        Intro = model.Intro,
                        Content = model.Content,
                        Picture = fileName
                    };

                    _context.Products.Add(newProduct);
                }
                else
                {
                    // Cập nhật
                    if (!Guid.TryParse(model.Id, out Guid productId))
                    {
                        return Json(new { success = false, message = "Mã sản phẩm không hợp lệ!" });
                    }

                    var existing = await _context.Products.FindAsync(productId);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                    }

                    existing.Title = model.Title.Trim();
                    existing.Price = model.Price ?? 0;
                    existing.CategoryId = categoryGuid;
                    existing.Intro = model.Intro;
                    existing.Content = model.Content;

                    // Chỉ đổi ảnh nếu có upload ảnh mới
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        existing.Picture = fileName;
                    }

                    _context.Products.Update(existing);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                // Trả về InnerException chi tiết từ SQL để dễ bắt lỗi
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi Database: " + innerMsg });
            }
        }

        // 4. Xóa Sản phẩm
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi khi xóa: " + innerMsg });
            }
        }
    }
}