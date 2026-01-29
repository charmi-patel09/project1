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
            var sourceLang = string.IsNullOrEmpty(request.SourceLanguage) ? "auto" : request.SourceLanguage;

            // Limit concurrency
            // Process sequentially to avoid 429 Rate Limiting from external API
            // Parallel Processing with Semaphore to speed up but respect API limits
            using var semaphore = new System.Threading.SemaphoreSlim(3); // Allow 3 concurrent requests
            var tasks = request.Texts.Distinct().Select(async text =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Pass explicit source language
                    var translated = await _translationService.TranslateAsync(text, request.TargetLanguage, sourceLang);
                    lock (results)
                    {
                        results[text] = translated;
                    }
                }
                catch
                {
                    lock (results)
                    {
                        results[text] = text;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            return Ok(results);
        }
    }

    public class BatchTranslationRequest
    {
        public List<string> Texts { get; set; }
        public string TargetLanguage { get; set; }
        public string SourceLanguage { get; set; } = "auto";
    }
}
