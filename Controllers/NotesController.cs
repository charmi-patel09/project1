using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;
using JsonCrudApp.Models;

namespace JsonCrudApp.Controllers
{
    [Route("[controller]/[action]")]
    public class NotesController : BaseController
    {
        private readonly NotesService _notesService;

        public NotesController(NotesService notesService)
        {
            _notesService = notesService;
        }

        private string? GetCurrentUserEmail()
        {
            return HttpContext.Session.GetString("AdminUser") ?? HttpContext.Session.GetString("StudentUser");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });

            var email = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var notes = _notesService.GetNotesByUser(email).OrderByDescending(n => n.CreatedDate);
            return Ok(notes);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Note note)
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });
            var email = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(note.Title) || string.IsNullOrWhiteSpace(note.Description))
            {
                return BadRequest("Title and Description are required.");
            }

            note.UserEmail = email;
            _notesService.AddNote(note);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Update([FromBody] Note note)
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });
            var email = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            note.UserEmail = email; // Ensure we only update for current user
            _notesService.UpdateNote(note);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete([FromBody] int id)
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });
            var email = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            _notesService.DeleteNote(id, email);
            return Ok(new { success = true });
        }
    }
}
