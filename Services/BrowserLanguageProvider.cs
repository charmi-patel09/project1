using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Linq;

namespace JsonCrudApp.Services
{
    public class BrowserLanguageProvider : RequestCultureProvider
    {
        private static readonly string[] SupportedLanguages = { "en", "hi", "gu" };

        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // 1. Get languages from Accept-Language header
            var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrEmpty(acceptLanguage))
            {
                return Task.FromResult<ProviderCultureResult?>(null);
            }

            // 2. Parse the header (e.g., "en-US,en;q=0.9,hi;q=0.8")
            var languages = acceptLanguage.Split(',')
                .Select(s => s.Split(';')[0].Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            foreach (var lang in languages)
            {
                // Check for exact match or parent match
                var primaryLang = lang.Split('-')[0].ToLower();
                if (SupportedLanguages.Contains(primaryLang))
                {
                    return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(primaryLang));
                }
            }

            // 3. Fallback to English if no match found
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult("en"));
        }
    }
}
