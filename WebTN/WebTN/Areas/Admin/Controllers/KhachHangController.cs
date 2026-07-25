using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code.Database.Models;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhachHangController : Controller
    {
        private readonly FoodContext _context;

        public KhachHangController(FoodContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH KHÁCH HÀNG
        public async Task<IActionResult> Index()
        {
            var listKhachHang = await _context.KhachHangs.ToListAsync();
            return View(listKhachHang);
        }

        // 2. THÊM MỚI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang khachHang)
        {
            ModelState.Remove("NgayTao");
            ModelState.Remove("TrangThai");

            if (ModelState.IsValid)
            {
                khachHang.NgayTao = DateTime.Now;
                khachHang.TrangThai = true;

                _context.Add(khachHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // 3. CHỈNH SỬA (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null) return NotFound();

            return View(khachHang);
        }

        // CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHang khachHang)
        {
            if (id != khachHang.MaKhachHang) return NotFound();

            ModelState.Remove("NgayTao");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.KhachHangs.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.HoTen = khachHang.HoTen;
                    existing.Email = khachHang.Email;
                    existing.SoDienThoai = khachHang.SoDienThoai;
                    existing.DiaChi = khachHang.DiaChi;
                    existing.TrangThai = khachHang.TrangThai;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KhachHangs.Any(e => e.MaKhachHang == khachHang.MaKhachHang))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // 4. XÓA KHÁCH HÀNG (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang != null)
            {
                _context.KhachHangs.Remove(khachHang);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}