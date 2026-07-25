using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly FoodContext _context;

        public OrderController(FoodContext context)
        {
            _context = context;
        }

        // Trang danh sách đơn hàng
        public IActionResult Index()
        {
            ViewBag.Title = "Quản lý Đơn hàng";
            return View();
        }

        // 1. Lấy danh sách đơn hàng cho DataTables (JSON)
        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedOn)
                .Select(o => new
                {
                    id = o.Id,
                    orderCode = o.OrderCode,
                    customerName = o.CustomerName,
                    customerPhone = o.CustomerPhone,
                    totalAmount = o.TotalAmount,
                    status = o.Status,
                    createdOn = o.CreatedOn.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new { data = orders });
        }

        // 2a. Trả về JSON chi tiết (Dùng cho Modal Xem & Modal Sửa)
        [HttpGet]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var details = order.OrderDetails?.Select(d => new
            {
                productName = d.Product != null ? d.Product.Title : "Sản phẩm đã xóa",
                price = d.Price,
                quantity = d.Quantity,
                total = d.Price * d.Quantity
            });

            return Json(new
            {
                id = order.Id,
                orderCode = order.OrderCode,
                customerName = order.CustomerName,
                customerPhone = order.CustomerPhone,
                customerAddress = order.CustomerAddress,
                note = order.Note,
                status = order.Status,
                totalAmount = order.TotalAmount,
                createdOn = order.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                items = details
            });
        }

        // 2b. Hiển thị View trang chi tiết đơn hàng (/Admin/Order/Detail/{id})
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // 3. Cập nhật trạng thái đơn hàng (AJAX & Form)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, int status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Detail), new { id = id });
        }

        // 4. Cập nhật đầy đủ thông tin đơn hàng (Dùng cho Modal Sửa)
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(Guid id, string customerName, string customerPhone, string customerAddress, string? note, int status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            order.CustomerName = customerName;
            order.CustomerPhone = customerPhone;
            order.CustomerAddress = customerAddress;
            order.Note = note;
            order.Status = status;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật đơn hàng thành công!" });
        }

        // 5. Xóa đơn hàng
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                _context.OrderDetails.RemoveRange(order.OrderDetails);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa đơn hàng thành công!" });
        }
    }
}