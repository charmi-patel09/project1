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

    public IActionResult Index()
    {
        var students = _studentService.GetStudents().ToList();

        ViewBag.TotalStudents = students.Count;
        ViewBag.AverageAge = students.Any() ? students.Average(s => s.Age ?? 0) : 0;
        ViewBag.RecentStudents = students.OrderByDescending(s => s.Id).Take(5).ToList();
        ViewBag.CourseStats = students.GroupBy(s => s.Course)
                                     .Select(g => new { Course = g.Key, Count = g.Count() })
                                     .ToList();

        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
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
