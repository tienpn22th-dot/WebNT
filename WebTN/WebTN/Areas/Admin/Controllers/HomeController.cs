using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Chuyển hướng trực tiếp sang Action Index của GroupController trong Area Admin
            return RedirectToAction("Index", "Group", new { area = "Admin" });
        }
    }
}