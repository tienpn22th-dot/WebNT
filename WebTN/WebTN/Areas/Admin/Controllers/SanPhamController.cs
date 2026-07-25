using Code.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamController : Controller
    {
        private readonly FoodContext _context;

        public SanPhamController(FoodContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH SẢN PHẨM
        public async Task<IActionResult> Index()
        {
            var listSanPham = await _context.SanPhams.Include(s => s.DanhMuc).ToListAsync();
            return View(listSanPham);
        }

        // 2. GIAO DIỆN THÊM MỚI (GET) - Đây là hàm ông đang thiếu!
        public IActionResult Create()
        {
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc");
            return View();
        }

        // 3. XỬ LÝ THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham)
        {
            // Bỏ qua validate thuộc tính điều hướng để tránh bị lỗi ModelState.IsValid = false
            ModelState.Remove("DanhMuc");

            if (ModelState.IsValid)
            {
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", sanPham.MaDanhMuc);
            return View(sanPham);
        }

        // 4. GIAO DIỆN CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();

            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", sanPham.MaDanhMuc);
            return View(sanPham);
        }

        // 5. XỬ LÝ CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sanPham)
        {
            if (id != sanPham.MaSp) return NotFound();

            ModelState.Remove("DanhMuc");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SanPhams.Any(e => e.MaSp == sanPham.MaSp)) return NotFound();
                    else throw;
                }
            }
            ViewBag.MaDanhMuc = new SelectList(_context.DanhMucs, "MaDanhMuc", "TenDanhMuc", sanPham.MaDanhMuc);
            return View(sanPham);
        }

        // 6. XỬ LÝ XÓA SẢN PHẨM
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();

            _context.SanPhams.Remove(sanPham);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}