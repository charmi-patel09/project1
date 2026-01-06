using JsonCrudApp.Models;
using JsonCrudApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonCrudApp.Controllers
{
    public class StudentsController : BaseController
    {
        private readonly JsonFileStudentService _studentService;

        public StudentsController(JsonFileStudentService studentService)
        {
            _studentService = studentService;
        }

        public IActionResult Index()
        {
            return View(_studentService.GetStudents());
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Create()
        {
            ModelState.Clear();
            return View(new Student());
        }

        [HttpPost]
        public IActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                _studentService.AddStudent(student);
                return RedirectToAction(nameof(Index));
            }
            // Always clear model state and return a blank model for a fresh UI
            ModelState.Clear();
            return View(new Student());
        }

        public IActionResult Edit(int id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                _studentService.UpdateStudent(student);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public IActionResult Details(int id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        public IActionResult Delete(int id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _studentService.DeleteStudent(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
