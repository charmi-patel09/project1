using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace JsonCrudApp.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EmergencyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetNumbers(string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest();

            // Normalize code
            code = code.Trim().ToUpper();

            // Override for India (IN) with correct national standards
            // API often returns outdated 100/102; 112 is the Unified Standard, 108 is Central Ambulance
            if (code == "IN")
            {
                var indiaData = "{\"data\":{\"country\":{\"name\":\"India\",\"isocode\":\"IN\"},\"dispatch\":{\"all\":[\"112\"]},\"police\":{\"all\":[\"112\"]},\"ambulance\":{\"all\":[\"108\"]},\"fire\":{\"all\":[\"101\"]},\"member_112\":true}}";
                return Content(indiaData, "application/json");
            }

            var client = _httpClientFactory.CreateClient();
            try
            {
                // Pass-through the API response directly to avoid CORS issues on frontend
                var response = await client.GetStringAsync($"https://emergencynumberapi.com/api/country/{code}");
                return Content(response, "application/json");
            }
            catch (Exception ex)
            {
                // Return a valid empty structure or error so frontend handles it gracefully
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
