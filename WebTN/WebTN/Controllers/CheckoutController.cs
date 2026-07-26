using Microsoft.AspNetCore.Mvc;
using Code.Database.Models;
using Web.Models.EF; // Đổi lại đúng namespace chứa FoodContext của ông
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

        // Trang hiển thị form thanh toán (/Checkout)
        public IActionResult Index()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }
            return View(cart);
        }

        // Xử lý lưu Đặt hàng vào DB
        [HttpPost]
        public IActionResult ProcessCheckout(string hoTen, string soDienThoai, string diaChi)
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 1. Lưu thông tin Đơn hàng
            var donHang = new DonHang
            {
                HoTen = hoTen,
                SoDienThoai = soDienThoai,
                DiaChi = diaChi,
                NgayDat = DateTime.Now,
                TongTien = cart.Sum(c => c.ThanhTien),
                TrangThai = "Chờ xác nhận"
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges(); // Lưu để sinh ra MaDh tự động

            // 2. Lưu Chi tiết từng món trong giỏ
            foreach (var item in cart)
            {
                var chiTiet = new ChiTietDonHang
                {
                    MaDh = donHang.MaDh,
                    MaSp = item.MaSp,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                };
                _context.ChiTietDonHangs.Add(chiTiet);
            }
            _context.SaveChanges();

            // 3. Xóa giỏ hàng sau khi đặt thành công
            ClearCart();

            return RedirectToAction("OrderSuccess", new { id = donHang.MaDh });
        }

        // Trang thông báo thành công
        public IActionResult OrderSuccess(int id)
        {
            ViewBag.MaDh = id;
            return View();
        }
    }
}