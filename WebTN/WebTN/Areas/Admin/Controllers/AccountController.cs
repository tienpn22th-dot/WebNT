using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly FoodContext _context;

        public AccountController(FoodContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public class AccountSaveModel
        {
            public string? Id { get; set; }
            public string LoginName { get; set; } = string.Empty;
            public string? Password { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Email { get; set; }
            public Guid? GroupId { get; set; }
        }

        // 1. Lấy danh sách tài khoản cho DataTables
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

                var query = _context.Members.Include(m => m.Group).AsQueryable();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(a => (a.LoginName != null && a.LoginName.Contains(searchValue)) ||
                                             (a.Name != null && a.Name.Contains(searchValue)) ||
                                             (a.Email != null && a.Email.Contains(searchValue)));
                }

                int recordsTotal = await query.CountAsync();

                var data = await query
                    .OrderByDescending(a => a.CreatedOn)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        id = a.Id,
                        loginName = a.LoginName ?? "—",
                        name = a.Name ?? "—",
                        email = a.Email ?? "—",
                        picture = a.Picture ?? "/images/default-avatar.png",
                        groupName = a.Group != null ? a.Group.Name : "Chưa phân nhóm",
                        createdOn = a.CreatedOn.HasValue ? a.CreatedOn.Value.ToString("dd/MM/yyyy HH:mm") : "—"
                    })
                    .ToListAsync();

                return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
            }
            catch (Exception ex)
            {
                return Json(new { draw = "1", recordsFiltered = 0, recordsTotal = 0, data = new List<object>(), error = ex.Message });
            }
        }

        // 2. Lấy thông tin 1 tài khoản để Sửa
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var acc = await _context.Members.FindAsync(id);
            if (acc == null) return NotFound();

            return Json(new
            {
                id = acc.Id,
                loginName = acc.LoginName,
                name = acc.Name,
                email = acc.Email,
                groupId = acc.GroupId
            });
        }

        // 3. Lưu Tài khoản (Thêm mới / Cập nhật)
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] AccountSaveModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.LoginName) || string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Vui lòng nhập Tên đăng nhập và Họ tên!" });
                }

                // Trường hợp THÊM MỚI
                if (string.IsNullOrEmpty(model.Id))
                {
                    if (string.IsNullOrWhiteSpace(model.Password))
                    {
                        return Json(new { success = false, message = "Vui lòng nhập Mật khẩu cho tài khoản mới!" });
                    }

                    var existUser = await _context.Members.AnyAsync(a => a.LoginName != null && a.LoginName.ToLower() == model.LoginName.Trim().ToLower());
                    if (existUser)
                    {
                        return Json(new { success = false, message = "Tên đăng nhập này đã tồn tại!" });
                    }

                    var newAcc = new Member
                    {
                        Id = Guid.NewGuid(),
                        LoginName = model.LoginName.Trim(),
                        Password = model.Password, // Lưu ý: thực tế nên Hash Password
                        Name = model.Name.Trim(),
                        Email = model.Email,
                        GroupId = model.GroupId,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now
                    };

                    _context.Members.Add(newAcc);
                }
                // Trường hợp CẬP NHẬT
                else
                {
                    if (!Guid.TryParse(model.Id, out Guid accId))
                    {
                        return Json(new { success = false, message = "Mã tài khoản không hợp lệ!" });
                    }

                    var existing = await _context.Members.FindAsync(accId);
                    if (existing == null) return Json(new { success = false, message = "Không tìm thấy tài khoản!" });

                    existing.Name = model.Name.Trim();
                    existing.Email = model.Email;
                    existing.GroupId = model.GroupId;
                    existing.ModifiedOn = DateTime.Now;

                    // Chỉ đổi mật khẩu nếu người dùng nhập mật khẩu mới
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        existing.Password = model.Password;
                    }

                    _context.Members.Update(existing);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi Database: " + innerMsg });
            }
        }

        // 4. Xóa tài khoản
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var acc = await _context.Members.FindAsync(id);
                if (acc == null) return Json(new { success = false, message = "Không tìm thấy tài khoản!" });

                _context.Members.Remove(acc);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi khi xóa: " + innerMsg });
            }
        }

        // 5. Lấy danh sách Nhóm / Quyền cho Dropdown Select
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            try
            {
                var groups = await _context.Groups
                    .Select(g => new
                    {
                        id = g.Id,
                        name = g.Name
                    })
                    .ToListAsync();

                return Json(groups);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }
    }
}