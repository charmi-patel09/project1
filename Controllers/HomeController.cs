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
