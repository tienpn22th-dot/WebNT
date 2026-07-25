using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly FoodContext _context;

        public UserController(FoodContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Groups = _context.Groups.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetList()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var query = _context.Members.Include(u => u.Group).AsQueryable();

            // Tìm kiếm theo Name, LoginName hoặc Email
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(u => (u.Name != null && u.Name.Contains(searchValue)) ||
                                         (u.LoginName != null && u.LoginName.Contains(searchValue)) ||
                                         (u.Email != null && u.Email.Contains(searchValue)));
            }

            int recordsTotal = await query.CountAsync();
            var data = await query.Skip(skip).Take(pageSize).Select(u => new
            {
                id = u.Id,
                name = u.Name,
                loginName = u.LoginName,
                email = u.Email,
                groupName = u.Group != null ? u.Group.Name : "Chưa gán",
                groupId = u.GroupId
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpPost]
        public async Task<IActionResult> Save(Member model)
        {
            try
            {
                // Kiểm tra Guid trống (Tạo mới)
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                    model.CreatedOn = DateTime.Now;
                    _context.Members.Add(model);
                }
                else
                {
                    // Cập nhật
                    var existingUser = await _context.Members.FindAsync(model.Id);
                    if (existingUser != null)
                    {
                        existingUser.Name = model.Name;
                        existingUser.LoginName = model.LoginName;
                        existingUser.Email = model.Email;
                        existingUser.GroupId = model.GroupId;
                        existingUser.ModifiedOn = DateTime.Now;

                        if (!string.IsNullOrEmpty(model.Password))
                        {
                            existingUser.Password = model.Password;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu dữ liệu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Members.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu!" });
            }

            _context.Members.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công!" });
        }
    }
}