using Code.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF;

namespace WebTN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucController : Controller
    {
        private readonly FoodContext _db;

        public DanhMucController(FoodContext db)
        {
            _db = db;
        }

        // GET: Admin/DanhMuc
        public async Task<IActionResult> Index()
        {
            var listDanhMuc = await _db.DanhMucs.ToListAsync();
            return View(listDanhMuc);
        }

        // GET: Admin/DanhMuc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/DanhMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                _db.DanhMucs.Add(danhMuc);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: Admin/DanhMuc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _db.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: Admin/DanhMuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMuc danhMuc)
        {
            if (id != danhMuc.MaDanhMuc) return NotFound();

            if (ModelState.IsValid)
            {
                _db.Update(danhMuc);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: Admin/DanhMuc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _db.DanhMucs.FirstOrDefaultAsync(m => m.MaDanhMuc == id);
            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        // POST: Admin/DanhMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _db.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                _db.DanhMucs.Remove(danhMuc);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}