using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers // Hoặc namespace Project của ông
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly FoodContext _context;

        public CategoryController(FoodContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Quản lý Danh mục sản phẩm";
            return View();
        }

        // 1. Lấy danh sách danh mục cho DataTables
        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var categories = await _context.Categories
                .Include(c => c.Parent)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    parentId = c.ParentId,
                    parentName = c.Parent != null ? c.Parent.Name : null,
                    createdOn = c.CreatedOn.HasValue ? c.CreatedOn.Value.ToString("dd/MM/yyyy HH:mm") : ""
                })
                .ToListAsync();

            return Json(new { data = categories });
        }

        // 2. Lấy danh sách làm Dropdown Danh mục cha (loại trừ chính nó để tránh đệ quy)
        [HttpGet]
        public async Task<IActionResult> GetParents(Guid? currentId)
        {
            var query = _context.Categories.AsQueryable();

            if (currentId.HasValue && currentId.Value != Guid.Empty)
            {
                query = query.Where(c => c.Id != currentId.Value);
            }

            var parents = await query
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();

            return Json(parents);
        }

        // 3. Lấy thông tin 1 danh mục theo ID
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _context.Categories
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    parentId = c.ParentId
                })
                .FirstOrDefaultAsync(c => c.id == id);

            if (category == null) return NotFound();

            return Json(category);
        }

        // 4. Thêm mới / Cập nhật
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CategorySaveModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Tên danh mục không được để trống!" });
            }

            if (model.Id == Guid.Empty)
            {
                // Thêm mới
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    ParentId = model.ParentId,
                    CreatedOn = DateTime.Now
                };

                _context.Categories.Add(category);
            }
            else
            {
                // Cập nhật
                var exist = await _context.Categories.FindAsync(model.Id);
                if (exist == null)
                    return Json(new { success = false, message = "Không tìm thấy danh mục!" });

                exist.Name = model.Name;
                exist.ParentId = model.ParentId;
                exist.ModifiedOn = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu thành công!" });
        }

        // 5. Xóa danh mục
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.ChildCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "Không tìm thấy danh mục!" });
            }

            // Kiểm tra nếu có danh mục con hoặc sản phẩm thì không cho xóa
            if (category.ChildCategories.Any())
            {
                return Json(new { success = false, message = "Không thể xóa! Danh mục này đang chứa các danh mục con." });
            }

            if (category.Products.Any())
            {
                return Json(new { success = false, message = "Không thể xóa! Danh mục này đang có sản phẩm thuộc về nó." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công!" });
        }
    }

    // DTO để nhận dữ liệu client gửi lên
    public class CategorySaveModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
    }
}