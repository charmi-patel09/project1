using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace JsonCrudApp.Services
{
    public class GlobalTranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public GlobalTranslationService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<string> TranslateAsync(string text, string targetLang, string sourceLang = "auto")
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // If source and target are strictly identical (and not auto), skip
            if (sourceLang != "auto" && sourceLang.Equals(targetLang, StringComparison.OrdinalIgnoreCase))
                return text;

            string cacheKey = $"trans_{sourceLang}_{targetLang}_{text.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out string cachedTranslation))
            {
                // Can contain null if cache corrupted?
                return cachedTranslation ?? text;
            }

            int maxRetries = 3;
            int delay = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Using the robust 'gtx' endpoint w/ user-defined source
                    var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={System.Web.HttpUtility.UrlEncode(text)}";

                    // Set User-Agent to avoid some blocks
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Increased timeout slightly
                    var response = await _httpClient.GetStringAsync(url, cts.Token);

                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    {
                        var sentences = root[0];
                        string fullTranslation = "";
                        foreach (var s in sentences.EnumerateArray())
                        {
                            if (s.ValueKind == JsonValueKind.Array && s.GetArrayLength() > 0)
                            {
                                fullTranslation += s[0].GetString();
                            }
                        }

                        if (!string.IsNullOrEmpty(fullTranslation))
                        {
                            // Fix blank strings
                            if (string.IsNullOrWhiteSpace(fullTranslation)) return text;

                            _cache.Set(cacheKey, fullTranslation, TimeSpan.FromHours(1));
                            return fullTranslation;
                        }
                    }

                    // If we got here, parse failed or empty. Break loop? No, retrying won't fix data format, but might fix bad gateway?
                    // Usually format error is permanent.
                    break;
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        Console.WriteLine($"Translation Error after {maxRetries} attempts: {ex.Message}");
                        // Return NULL to indicate failure so controller can handle it (or handle gracefully here)
                        // User req: "Do not return the original input text as a fallback unless the API fails."
                        // We are returning original on fatal error.
                        return text;
                    }
                    await Task.Delay(delay * (i + 1));
                }
            }

            return text;
        }
    }
}
