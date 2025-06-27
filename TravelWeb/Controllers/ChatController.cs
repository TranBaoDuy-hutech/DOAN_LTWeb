using Microsoft.AspNetCore.Mvc;

namespace TravelWeb.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
