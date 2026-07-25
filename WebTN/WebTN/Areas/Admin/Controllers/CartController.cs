using Microsoft.AspNetCore.Mvc;
using Code.Database.Models;
using Web.Models.EF; // Hoặc namespace chứa FoodContext của ông
using WebTN.Helpers;

namespace WebTN.Controllers
{
    public class CartController : Controller
    {
        private readonly FoodContext _context;
        private const string CART_KEY = "MyCart";

        public CartController(FoodContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCartItems()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_KEY);
            return cart ?? new List<CartItem>();
        }

        private void SaveCartItems(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
        }

        // 1. API Thêm vào giỏ (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MaSp == id);

            if (item != null)
            {
                item.SoLuong += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MaSp = sp.MaSp,
                    TenSp = sp.TenSp,
                    Gia = sp.Gia,
                    Anh = sp.Anh ?? "",
                    SoLuong = quantity
                });
            }

            SaveCartItems(cart);

            int totalQuantity = cart.Sum(c => c.SoLuong);
            decimal totalMoney = cart.Sum(c => c.ThanhTien);

            return Json(new
            {
                success = true,
                totalQuantity = totalQuantity,
                totalMoney = string.Format("{0:N0} đ", totalMoney)
            });
        }

        // 2. Trang danh sách Giỏ hàng (/Cart)
        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        // 3. Cập nhật số lượng sản phẩm trong giỏ (AJAX)
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MaSp == id);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.SoLuong = quantity;
                }
                else
                {
                    cart.Remove(item); // Nếu chỉnh số lượng <= 0 thì xóa luôn
                }
                SaveCartItems(cart);
            }

            decimal itemTotal = item != null ? item.ThanhTien : 0;
            decimal totalMoney = cart.Sum(c => c.ThanhTien);
            int totalQuantity = cart.Sum(c => c.SoLuong);

            return Json(new
            {
                success = true,
                itemTotal = string.Format("{0:N0} đ", itemTotal),
                totalMoney = string.Format("{0:N0} đ", totalMoney),
                totalQuantity = totalQuantity
            });
        }

        // 4. Xóa 1 món khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MaSp == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartItems(cart);
            }

            decimal totalMoney = cart.Sum(c => c.ThanhTien);
            int totalQuantity = cart.Sum(c => c.SoLuong);

            return Json(new
            {
                success = true,
                totalMoney = string.Format("{0:N0} đ", totalMoney),
                totalQuantity = totalQuantity
            });
        }
    }
}