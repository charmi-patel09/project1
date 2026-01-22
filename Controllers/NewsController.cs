using Microsoft.AspNetCore.Mvc;

namespace JsonCrudApp.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Details(string? query, string? fallbackQuery = null)
        {
            ViewData["Query"] = query;
            ViewData["FallbackQuery"] = fallbackQuery;
            return View();
        }
    }
}
