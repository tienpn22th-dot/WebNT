using Microsoft.AspNetCore.Mvc;

namespace WebTN.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}