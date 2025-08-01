using Microsoft.AspNetCore.Mvc;

namespace bike.Controllers
{
    public class SharedController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 