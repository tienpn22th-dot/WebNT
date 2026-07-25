using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core; // Bắt buộc phải có để sử dụng OrderBy(string)
using Web.Areas.Admin.Models;
using Web.Models.EF;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GroupController : Controller
    {
        private readonly FoodContext _dbContext;

        public GroupController(FoodContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Save(Guid? id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Tên nhóm quyền không được để trống!" });
            }

            if (id == null || id == Guid.Empty)
            {
                // Thêm mới nhóm quyền
                var group = new Core.Database.Models.Group
                {
                    Id = Guid.NewGuid(),
                    Name = name.Trim()
                };
                _dbContext.Groups.Add(group);
            }
            else
            {
                // Cập nhật thông tin nhóm quyền
                var group = await _dbContext.Groups.FindAsync(id);
                if (group == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dữ liệu!" });
                }
                group.Name = name.Trim();
                _dbContext.Groups.Update(group);
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu dữ liệu thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var group = await _dbContext.Groups.FindAsync(id);
            if (group == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu cần xóa!" });
            }

            _dbContext.Groups.Remove(group);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> getList(jDatatable model)
        {
            var items = _dbContext.Groups.AsQueryable();

            // 1. Đếm tổng số bản ghi gốc
            int recordsTotal = await items.CountAsync();

            // 2. Tìm kiếm (Search)
            if (model.search != null && !string.IsNullOrWhiteSpace(model.search.value))
            {
                var searchValue = model.search.value.Trim();
                items = items.Where(i => i.Name != null && i.Name.Contains(searchValue));
            }

            // 3. Đếm số lượng bản ghi sau khi lọc
            int recordsFiltered = await items.CountAsync();

            // 4. Sắp xếp động (Dynamic OrderBy)
            bool isOrdered = false;
            if (model.order != null && model.order.Count > 0 && model.columns != null)
            {
                var columnIndex = model.order[0].column;
                if (columnIndex < model.columns.Count)
                {
                    var columnName = model.columns[columnIndex].name;
                    var dir = model.order[0].dir;

                    if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(dir))
                    {
                        items = items.OrderBy($"{columnName} {dir}");
                        isOrdered = true;
                    }
                }
            }

            // Nếu frontend không gửi tên cột hợp lệ, tự sắp xếp mặc định theo Name
            if (!isOrdered)
            {
                items = items.OrderBy(i => i.Name);
            }

            // 5. Phân trang và Lấy dữ liệu
            var data = await items.Select(i => new
            {
                i.Id,
                i.Name
            })
            .Skip(model.start)
            .Take(model.length)
            .ToListAsync();

            // 6. Trả về đúng định dạng JSON DataTables
            return Ok(new
            {
                draw = model.draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }
    }
}