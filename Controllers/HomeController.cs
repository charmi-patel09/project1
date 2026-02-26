using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Models;
using JsonCrudApp.Services;

namespace JsonCrudApp.Controllers;

public class HomeController : BaseController
{
    private readonly JsonFileStudentService _studentService;
    private readonly RoleWidgetService _roleWidgetService;
    private readonly UserWidgetService _userWidgetService;

    public HomeController(JsonFileStudentService studentService, RoleWidgetService roleWidgetService, UserWidgetService userWidgetService)
    {
        _studentService = studentService;
        _roleWidgetService = roleWidgetService;
        _userWidgetService = userWidgetService;
    }

    public IActionResult Dashboard()
    {
        var email = HttpContext.Session.GetString("StudentUser");
        if (!string.IsNullOrEmpty(email))
        {
            var user = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);
            if (user != null)
            {
                // Strict Role-Based Widget Logic
                var roleWidgets = _roleWidgetService.GetWidgetsByRole(user.Role.ToString());
                ViewBag.WidgetPermissions = roleWidgets?.AllowedWidgets;

                ViewBag.Role = user.Role.ToString();
                ViewBag.IsSecurityEnabled = user.Role == UserRole.Visitor ? false : user.IsSecurityEnabled;
                ViewBag.IsAdmin = user.Role == UserRole.Admin;

                // Per-widget unlock state: Unlocked if Security is disabled OR Session marked as unlocked OR User is Visitor
                bool isVisitor = user.Role == UserRole.Visitor;
                ViewBag.IsHabitUnlocked = isVisitor || !user.IsSecurityEnabled || HttpContext.Session.GetString("Unlocked_habit-hub") == "true";
                ViewBag.IsPdfUnlocked = isVisitor || !user.IsSecurityEnabled || HttpContext.Session.GetString("Unlocked_pdf-hub") == "true";
                ViewBag.IsNotesUnlocked = isVisitor || !user.IsSecurityEnabled || HttpContext.Session.GetString("Unlocked_notes-hub") == "true";

                // Keep global flags false to ensure individual checks are used
                ViewBag.IsUnlocked = false;
                ViewBag.PinVerified = false;

                // Log only on state change or once per "session load" (simulated by checking if we already logged it in this request's context if we had one)
                // For now, let's just significantly reduce it.
            }
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
