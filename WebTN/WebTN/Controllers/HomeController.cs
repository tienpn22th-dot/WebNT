using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models.EF; // Namespace ch?a FoodContext
using WebTN.Models;

namespace WebTN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FoodContext _context;

        public HomeController(ILogger<HomeController> logger, FoodContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // L?y danh sách s?n ph?m t? b?ng SanPham
            var listProduct = await _context.SanPhams.ToListAsync();

            // Truy?n d? li?u sang View Index.cshtml
            return View(listProduct);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}