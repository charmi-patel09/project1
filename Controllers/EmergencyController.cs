using Microsoft.AspNetCore.Mvc;
using JsonCrudApp.Services;

namespace JsonCrudApp.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly EmergencyService _service;

        public EmergencyController(EmergencyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetNumbers(string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Code required");

            var info = await _service.GetByCountryCodeAsync(code);

            if (info == null)
            {
                // Fallback or empty
                return NotFound(new { message = "Country not found in database" });
            }

            // Normalizing structure for frontend
            var data = new
            {
                dispatch = new { all = info.Dispatch },
                police = new { all = info.Police },
                ambulance = new { all = info.Ambulance },
                fire = new { all = info.Fire }
            };

            return Ok(new { data = data });
        }
    }
}
