using Code.Database.Models;
using Core.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Web.Models.EF;
using WebTN.Helpers;

namespace WebTN.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly FoodContext _context;
        private const string CART_KEY = "MyCart";

        public CheckoutController(FoodContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCartItems()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY);
            return cart ?? new List<CartItem>();
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CART_KEY);
        }

        public IActionResult Index()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(string hoTen, string soDienThoai, string diaChi, string? note, string phuongThucTT)
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                // 1. Tạo Đơn hàng chuẩn Model Order
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CustomerName = hoTen ?? "Khách lẻ",
                    CustomerPhone = soDienThoai ?? "",
                    CustomerAddress = diaChi ?? "",
                    Note = string.IsNullOrEmpty(note) ? $"PTTT: {phuongThucTT ?? "COD"}" : $"{note} (PTTT: {phuongThucTT ?? "COD"})", // Lưu PTTT vào Note nếu Model Order không có trường PaymentMethod
                    TotalAmount = (double)cart.Sum(c => c.ThanhTien),
                    Status = 0, // 0: Chờ duyệt (Khớp với View Admin)
                    CreatedOn = DateTime.Now
                };

                _context.Orders.Add(order);

                // 2. Tạo Chi tiết đơn hàng (Model OrderDetail)
                foreach (var item in cart)
                {
                    Guid productId;
                    if (!Guid.TryParse(item.MaSp.ToString(), out productId))
                    {
                        var product = _context.Products.FirstOrDefault(p => p.Id.ToString() == item.MaSp.ToString());
                        productId = product != null ? product.Id : _context.Products.Select(p => p.Id).FirstOrDefault();
                    }

                    var orderDetail = new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = productId,
                        Price = (double)item.Gia,
                        Quantity = item.SoLuong
                    };

                    _context.OrderDetails.Add(orderDetail);
                }

                // 3. Lưu vào Database
                await _context.SaveChangesAsync();

                // 4. Xóa giỏ hàng sau khi lưu đơn thành công
                ClearCart();

                return RedirectToAction("OrderSuccess", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        public async Task<IActionResult> OrderSuccess(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return RedirectToAction("Index", "Home");

            // Truyền cả Model và OrderCode qua ViewBag cho chắc ăn
            ViewBag.OrderCode = order.OrderCode;
            return View(order);
        }
    }
}