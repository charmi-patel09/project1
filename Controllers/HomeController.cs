using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Models;
using JsonCrudApp.Services;

namespace JsonCrudApp.Controllers;

public class HomeController : BaseController
{
    private readonly JsonFileStudentService _studentService;

    public HomeController(JsonFileStudentService studentService)
    {
        _studentService = studentService;
    }



    public IActionResult Dashboard()
    {
        var email = HttpContext.Session.GetString("StudentUser");
        if (!string.IsNullOrEmpty(email))
        {
            var user = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);
            if (user != null)
            {
                ViewBag.WidgetPermissions = user.WidgetPermissions;
                ViewBag.Role = user.Role;
                ViewBag.HasPin = user.IsSecurityEnabled;
            }
            // Require PIN on page refresh/initial load
            HttpContext.Session.SetString("PinVerified", "false");
        }
        return View();
    }

    public IActionResult CountryDetails(string name)
    {
        ViewData["SearchQuery"] = name;
        return View();
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
