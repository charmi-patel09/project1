using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;
using JsonCrudApp.Models;
using System.Linq;

namespace JsonCrudApp.Controllers
{
    public class RoleWidgetsController : BaseController
    {
        private readonly RoleWidgetService _roleWidgetService;
        private readonly JsonFileStudentService _studentService;

        public RoleWidgetsController(RoleWidgetService roleWidgetService, JsonFileStudentService studentService)
        {
            _roleWidgetService = roleWidgetService;
            _studentService = studentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("StudentUser");
            var user = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);

            if (user == null || user.Role != UserRole.Admin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Fetch all roles except legacy 'normal' role
            var roleWidgets = _roleWidgetService.GetRoleWidgets()
                .Where(rw => !rw.Role.Equals("normal", System.StringComparison.OrdinalIgnoreCase));

            return View(roleWidgets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(string role, string[] widgets)
        {
            var email = HttpContext.Session.GetString("StudentUser");
            var user = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);

            if (user == null || user.Role != UserRole.Admin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // ATOMIC UPDATE: Replace old mappings with newly selected widgets for specific role
            string widgetList = string.Join(",", widgets ?? System.Array.Empty<string>());
            _roleWidgetService.UpdateRoleWidgets(role, widgetList);

            TempData["SuccessMessage"] = $"Permissions for role '{role}' updated successfully. All users under this role will now reflect these changes.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string roleName)
        {
            var email = HttpContext.Session.GetString("StudentUser");
            var user = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);

            if (user == null || user.Role != UserRole.Admin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Role name cannot be empty.";
                return RedirectToAction("Index");
            }

            _roleWidgetService.CreateRole(roleName.Trim());

            TempData["SuccessMessage"] = $"Role '{roleName}' created successfully with all widgets assigned by default.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string role)
        {
            var email = HttpContext.Session.GetString("StudentUser");
            var adminUser = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);

            if (adminUser == null || adminUser.Role != UserRole.Admin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var allRoles = _roleWidgetService.GetRoleWidgets().ToList();
            if (allRoles.Count <= 1)
            {
                TempData["ErrorMessage"] = "Cannot delete the last remaining role. At least one role must exist in the system.";
                return RedirectToAction("Index");
            }

            // Determine dynamic fallback role (any role that is NOT the one being deleted)
            var fallbackRole = allRoles.FirstOrDefault(r => !r.Role.Equals(role, StringComparison.OrdinalIgnoreCase))?.Role ?? "Visitor";

            // SAFE HANDLING: Reassign all affected users to the fallback role
            var affectedUsers = _studentService.GetStudents().Where(s => s.Role.Equals(role, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var userEntry in affectedUsers)
            {
                userEntry.Role = fallbackRole;
                _studentService.UpdateStudent(userEntry);
            }

            _roleWidgetService.DeleteRole(role);

            TempData["SuccessMessage"] = $"Role '{role}' deleted successfully. {affectedUsers.Count} users were reassigned to the '{fallbackRole}' role.";
            return RedirectToAction("Index");
        }
    }
}
