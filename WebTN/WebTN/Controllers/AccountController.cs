using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;
using Core.Database.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebTN.Controllers
{
    public class AccountController : Controller
    {
        private readonly FoodContext _context;

        public AccountController(FoodContext context)
        {
            _context = context;
        }

        // Hàm hỗ trợ mã hóa MD5
        private string GetMD5Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string loginName, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(loginName) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!";
                return View();
            }

            string inputLogin = loginName.Trim().ToLower();
            string rawPassword = password.Trim();
            string md5Password = GetMD5Hash(rawPassword);

            // Tìm user theo LoginName hoặc Email, chấp nhận cả Pass thô lẫn Pass đã mã hóa MD5
            var user = await _context.Members
                .Include(m => m.Group)
                .FirstOrDefaultAsync(m =>
                    (m.LoginName.ToLower() == inputLogin || (m.Email != null && m.Email.ToLower() == inputLogin))
                    && (m.Password == rawPassword || m.Password == md5Password)
                );

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
            }

            // Lưu Session Đăng nhập
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name ?? user.LoginName ?? "User");
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserRole", user.Group?.Name ?? "Customer");

            // Điều hướng dựa trên quyền
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Group != null && (user.Group.Name.Contains("Admin") || user.Group.Name.Contains("Quản trị")))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string name, string loginName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var existUser = await _context.Members.AnyAsync(m => m.LoginName.ToLower() == loginName.Trim().ToLower());
            if (existUser)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            var newUser = new Member
            {
                Id = Guid.NewGuid(),
                Name = name,
                LoginName = loginName.Trim(),
                Email = email,
                Password = GetMD5Hash(password.Trim()), // Lưu mật khẩu dạng MD5 cho an toàn
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now
            };

            _context.Members.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}