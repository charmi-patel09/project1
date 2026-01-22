using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace JsonCrudApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class TranslationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly string? _baseUrl;

        public TranslationController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = _config["TranslationSettings:ApiKey"];
            _baseUrl = _config["TranslationSettings:BaseUrl"];
        }

        private bool IsConfigured => !string.IsNullOrEmpty(_baseUrl);

        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            if (IsConfigured)
            {
                try
                {
                    // LibreTranslate Languages Endpoint
                    if (_baseUrl.Contains("libretranslate"))
                    {
                        var response = await _httpClient.GetAsync(_baseUrl.Replace("/translate", "/languages"));
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            // LibreTranslate returns [{ code: "en", name: "English" }, ... ]
                            // We need to map it if the frontend expects a specific format, or just return it?
                            // Frontend likely expects { data: { languages: [] } } if it follows Google format.
                            using var doc = JsonDocument.Parse(content);
                            var langs = new List<object>();
                            foreach (var l in doc.RootElement.EnumerateArray())
                            {
                                langs.Add(new { language = l.GetProperty("code").GetString(), name = l.GetProperty("name").GetString() });
                            }
                            return Ok(JsonSerializer.Serialize(new { data = new { languages = langs } }));
                        }
                    }
                    else
                    {
                        // Google Format
                        var url = $"{_baseUrl}/languages?target=en";
                        if (!string.IsNullOrEmpty(_apiKey)) url += $"&key={_apiKey}";

                        var response = await _httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            return Ok(content);
                        }
                    }
                }
                catch
                {
                    // Fallback to static list on error
                }
            }

            // Return static list for free tier or error fallback
            var staticLanguages = new
            {
                data = new
                {
                    languages = new[]
                    {
                        new { language = "en", name = "English" },
                        new { language = "es", name = "Spanish" },
                        new { language = "fr", name = "French" },
                        new { language = "de", name = "German" },
                        new { language = "zh", name = "Chinese (Simplified)" },
                        new { language = "hi", name = "Hindi" },
                        new { language = "gu", name = "Gujarati" },
                        new { language = "ar", name = "Arabic" },
                        new { language = "ru", name = "Russian" },
                        new { language = "ja", name = "Japanese" },
                        new { language = "pt", name = "Portuguese" },
                        new { language = "it", name = "Italian" }
                    }
                }
            };
            return Ok(JsonSerializer.Serialize(staticLanguages));
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            try 
            {
                if (request.Q == null || request.Target == null)
                {
                    return BadRequest("Invalid request body.");
                }

                if (IsConfigured && _baseUrl != null)
                {
                    try
                    {
                        if (_baseUrl.Contains("libretranslate"))
                        {
                            var translatedTexts = new List<string>();
                            foreach(var text in request.Q)
                            {
                                if (string.IsNullOrWhiteSpace(text)) {
                                    translatedTexts.Add(text ?? "");
                                    continue;
                                }

                                var payload = new
                                {
                                    q = text,
                                    target = request.Target,
                                    source = "auto",
                                    format = "text",
                                    api_key = _apiKey ?? ""
                                };
                                
                                var json = JsonSerializer.Serialize(payload);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                
                                // We are inside a loop, ensure we don't bombard the server if it's down.
                                // Minimal timeout or error handling?
                                // Default httpClient timeout is long.
                                
                                var response = await _httpClient.PostAsync(_baseUrl, content);
                                
                                if (response.IsSuccessStatusCode)
                                {
                                    var resStr = await response.Content.ReadAsStringAsync();
                                    using var doc = JsonDocument.Parse(resStr);
                                    if(doc.RootElement.TryGetProperty("translatedText", out var tProp))
                                    {
                                        translatedTexts.Add(tProp.GetString() ?? text);
                                    }
                                    else 
                                    {
                                        translatedTexts.Add(text);
                                    }
                                }
                                else
                                {
                                    translatedTexts.Add(text);
                                }
                            }
                            
                            var result = new
                            {
                                data = new
                                {
                                    translations = translatedTexts.Select(t => new { translatedText = t }).ToArray()
                                }
                            };
                            return Ok(JsonSerializer.Serialize(result));
                        }
                        else
                        {
                            // Google Cloud Translation API
                            var payload = new
                            {
                                q = request.Q,
                                target = request.Target,
                                format = "text"
                            };

                            var json = JsonSerializer.Serialize(payload);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            
                            var url = _baseUrl;
                            if(!string.IsNullOrEmpty(_apiKey)) url += $"?key={_apiKey}";

                            var response = await _httpClient.PostAsync(url, content);

                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                return Ok(responseString);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        // Log error and fall through to free tier
                        System.Diagnostics.Debug.WriteLine($"Official API failed: {ex.Message}");
                    }
                }

                // Fallback: Free 'GTX' Endpoint
                try
                {
                    var normalizedTarget = (request.Target.Length > 2) ? request.Target.Substring(0, 2) : request.Target;
                    var translatedTexts = new List<string>();

                    // Process sequentially to be polite to the free endpoint
                    foreach (var text in request.Q)
                    {
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            translatedTexts.Add(text ?? "");
                            continue;
                        }

                        try 
                        {
                            // Use GTX endpoint
                            var textEncoded = System.Net.WebUtility.UrlEncode(text);
                            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={normalizedTarget}&dt=t&q={textEncoded}";
                            
                            // add user agent to avoid immediate block
                            var requestMsg = new HttpRequestMessage(HttpMethod.Get, url);
                            requestMsg.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                            
                            var res = await _httpClient.SendAsync(requestMsg);
                            if (res.IsSuccessStatusCode) 
                            {
                                var resBody = await res.Content.ReadAsStringAsync();

                                // Parse GTX response: [[["Translated","Original",...],...],...]
                                using var doc = JsonDocument.Parse(resBody);
                                var root = doc.RootElement;

                                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                                {
                                    var sentences = root[0];
                                    var sb = new StringBuilder();
                                    foreach (var token in sentences.EnumerateArray())
                                    {
                                        if (token.ValueKind == JsonValueKind.Array && token.GetArrayLength() > 0)
                                        {
                                            sb.Append(token[0].GetString());
                                        }
                                    }
                                    translatedTexts.Add(sb.ToString());
                                }
                                else
                                {
                                    translatedTexts.Add(text); // Fail safe
                                }
                            }
                            else 
                            {
                                translatedTexts.Add(text);
                            }
                        }
                        catch
                        {
                            translatedTexts.Add(text); // Individual text failure
                        }
                    }

                    // Construct V2-compatible response
                    var result = new
                    {
                        data = new
                        {
                            translations = translatedTexts.Select(t => new { translatedText = t }).ToArray()
                        }
                    };

                    return Ok(JsonSerializer.Serialize(result));
                }
                catch (System.Exception ex)
                {
                   // Fallback crash? Return originals
                   var result = new
                   {
                        data = new
                        {
                            translations = request.Q.Select(t => new { translatedText = t }).ToArray()
                        }
                   };
                   return Ok(JsonSerializer.Serialize(result));
                }
            }
            catch (Exception ex)
            {
                // Top Level Catch - Ensures 500 is NEVER thrown, returns originals + error in header maybe?
               var result = new
               {
                    data = new
                    {
                        translations = (request.Q ?? Array.Empty<string>()).Select(t => new { translatedText = t }).ToArray()
                    }
               };
               // Optionally log real error
               Console.WriteLine("CRITICAL TRANSLATION ERROR: " + ex.ToString());
               return Ok(JsonSerializer.Serialize(result));
            }
        }
    }

    public class TranslateRequest
    {
        public string[]? Q { get; set; }
        public string? Target { get; set; }
    }
}
