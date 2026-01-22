using JsonCrudApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonCrudApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly GlobalTranslationService _translationService;

        public TranslationController(GlobalTranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost("translate-batch")]
        public async Task<IActionResult> TranslateBatch([FromBody] BatchTranslationRequest request)
        {
            if (request == null || request.Texts == null || request.Texts.Count == 0)
                return BadRequest("No text provided");

            var results = new Dictionary<string, string>();

            // Limit concurrency
            var tasks = request.Texts.Distinct().Select(async text =>
            {
                var translated = await _translationService.TranslateAsync(text, request.TargetLanguage);
                return new { Original = text, Translated = translated };
            });

            var pairs = await Task.WhenAll(tasks);

            foreach (var pair in pairs)
            {
                results[pair.Original] = pair.Translated;
            }

            return Ok(results);
        }
    }

    public class BatchTranslationRequest
    {
        public List<string> Texts { get; set; }
        public string TargetLanguage { get; set; }
    }
}
