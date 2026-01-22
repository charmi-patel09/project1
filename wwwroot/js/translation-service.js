/**
 * Global Translation Service
 * Handles API-based translation, caching, and DOM manipulation.
 */

class TranslationService {
    constructor() {
        // =============================================================
        // CONFIGURATION
        // =============================================================
        this.apiKey = 'YOUR_GOOGLE_TRANSLATE_API_KEY'; // Placeholder (Used on backend now)
        this.baseUrl = 'https://translation.googleapis.com/language/translate/v2';

        // State
        this.currentLang = localStorage.getItem('app_lang') || 'en';
        this.cache = JSON.parse(localStorage.getItem('trans_cache') || '{}');
        this.observer = null;
        this.isTranslating = false;

        // RTL Languages Map
        this.rtlLangs = ['ar', 'he', 'ur', 'fa', 'ps', 'sd'];

        this.init();
    }

    init() {
        console.log('[TranslationService] Initialized. Current Lang:', this.currentLang);
        window.translator = this;

        // Visual Direction updates
        this.applyDirection();

        // Auto-Detect for First Time User
        if (!localStorage.getItem('app_lang')) {
            const browserLang = navigator.language || navigator.userLanguage;
            // Map "en-US" -> "en"
            const detectCode = browserLang.split('-')[0].toLowerCase();
            this.currentLang = detectCode;
            localStorage.setItem('app_lang', detectCode);
        }

        // Translation Kickoff
        if (this.currentLang !== 'en') {
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', () => this.startTranslationFlow());
            } else {
                this.startTranslationFlow();
            }
        }
    }

    startTranslationFlow() {
        // Initial Page Translate
        this.translatePage();

        // Watch for dynamic content (Widgets, Modals, etc.)
        this.startObserver();
    }

    applyDirection() {
        const isRtl = this.rtlLangs.includes(this.currentLang);
        document.documentElement.dir = isRtl ? 'rtl' : 'ltr';
        document.documentElement.lang = this.currentLang;
    }

    /**
     * Fetches supported languages from Google API
     */
    async getSupportedLanguages() {
        const cached = localStorage.getItem('supported_langs');
        if (cached) return JSON.parse(cached);

        try {
            // Call Local Backend Proxy
            const res = await fetch('/api/Translation/languages');

            if (!res.ok) {
                // Fallback if server isn't configured yet
                console.warn('[TranslationService] Server responded with error, using mocks.');
                return this.getMockLanguages();
            }

            const data = await res.json();

            // Google API structure: { data: { languages: [...] } }
            // Verify structure matches what proxy returns
            const langs = (data.data && data.data.languages) ? data.data.languages : [];

            if (langs.length > 0) {
                localStorage.setItem('supported_langs', JSON.stringify(langs));
                return langs;
            }
            return this.getMockLanguages();
        } catch (e) {
            console.error('Failed to fetch languages from backend:', e);
            return this.getMockLanguages();
        }
    }

    getMockLanguages() {
        return [
            { language: 'en', name: 'English' },
            { language: 'es', name: 'Spanish' },
            { language: 'fr', name: 'French' },
            { language: 'de', name: 'German' },
            { language: 'zh', name: 'Chinese (Simplified)' },
            { language: 'hi', name: 'Hindi' },
            { language: 'gu', name: 'Gujarati' },
            { language: 'ar', name: 'Arabic' },
            { language: 'ru', name: 'Russian' },
            { language: 'ja', name: 'Japanese' }
        ];
    }

    /**
     * Switches globally to a new language
     */
    async setLanguage(langCode) {
        if (this.currentLang === langCode) return;

        console.log(`Switching language to: ${langCode}`);
        this.currentLang = langCode;
        localStorage.setItem('app_lang', langCode);

        // Sync Legacy Shim
        if (window.i18n) window.i18n.locale = langCode;

        // Trigger Legacy jQuery Event
        if (window.jQuery) {
            window.jQuery(document).trigger('languageChanged', [langCode]);
        }

        this.applyDirection();

        // Always trigger translation flow. 
        // If 'en', translatePage will handle restoration from _originalText
        await this.translatePage();
        this.startObserver();
    }

    async translatePage() {
        if (this.isTranslating) return;
        this.isTranslating = true;
        document.body.classList.add('app-translating');

        try {
            const textNodes = this.collectTextNodes(document.body);

            // If switching to English, restore originals and skip API
            if (this.currentLang === 'en') {
                textNodes.forEach(node => {
                    if (node.el._originalText) {
                        if (node.type === 'text') node.el.nodeValue = node.el._originalText;
                        else if (node.type === 'attr') node.el.setAttribute(node.attr, node.el._originalText);
                    }
                });
                return;
            }

            // identify strings needed for API
            const uniqueTexts = [...new Set(textNodes.map(n => n.originalText).filter(t => t.trim().length > 0))];

            // Check cache
            const toTranslate = uniqueTexts.filter(t => !this.cache[this.currentLang]?.[t]);

            // Skip purely numeric/special chars strings from API calls (optional optimization)
            // But user said "Translate EVERYTHING", so we interpret that as 'let API decide' 
            // EXCEPT for things that look like code or chaos. 
            // For now, we stick to the plan.

            if (toTranslate.length > 0) {
                console.log(`[Translation] Fetching ${toTranslate.length} new strings...`);
                await this.fetchTranslations(toTranslate);
            }

            // Apply translations
            textNodes.forEach(node => {
                const trans = this.getCached(node.originalText);
                if (trans) {
                    if (node.type === 'text') node.el.nodeValue = trans;
                    else if (node.type === 'attr') node.el.setAttribute(node.attr, trans);
                }
            });
        } finally {
            this.isTranslating = false;
            document.body.classList.remove('app-translating');
            document.dispatchEvent(new CustomEvent('pageTranslated', { detail: this.currentLang }));
        }
    }

    collectTextNodes(root) {
        const nodes = [];
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT, null, false);

        while (walker.nextNode()) {
            const node = walker.currentNode;

            if (['SCRIPT', 'STYLE', 'CODE', 'PRE', 'NOSCRIPT'].includes(node.parentNode?.tagName)) continue;
            if (node.parentNode?.classList?.contains('no-translate')) continue;
            if (node.classList?.contains('no-translate')) continue;

            if (node.nodeType === Node.TEXT_NODE) {
                const text = node.nodeValue.trim();
                if (text) {
                    // STORE ORIGINAL ENGLISH TEXT ONCE
                    if (!node._originalText) node._originalText = node.nodeValue; // Keep formatting spaces for restore

                    nodes.push({ type: 'text', el: node, originalText: node._originalText.trim() });
                }
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                ['placeholder', 'title', 'alt', 'value', 'aria-label', 'data-tooltip', 'data-original-title'].forEach(attr => {
                    if (node.hasAttribute(attr)) {
                        if (attr === 'value' && !['submit', 'button', 'reset', 'input'].includes(node.tagName.toLowerCase()) && !['submit', 'button', 'reset'].includes(node.type)) return;

                        const val = node.getAttribute(attr);
                        if (val && val.trim()) {
                            // Store original on the element using a property map if possible, or data-attr
                            // Using a property implies we need a robust key. 
                            const propName = `_original_${attr}`;
                            if (!node[propName]) node[propName] = val;

                            nodes.push({ type: 'attr', el: node, attr: attr, originalText: node[propName].trim() });
                        }
                    }
                });
            }
        }
        return nodes;
    }

    async fetchTranslations(texts) {
        // Chunk sizes to respect API limits
        const chunkSize = 50;

        for (let i = 0; i < texts.length; i += chunkSize) {
            const chunk = texts.slice(i, i + chunkSize);
            let success = false;
            let attempts = 0;

            while (!success && attempts < 3) {
                try {
                    attempts++;
                    // Call Local Backend Proxy
                    const response = await fetch('/api/Translation/translate', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            Q: chunk,
                            Target: this.currentLang
                        })
                    });

                    if (!response.ok) {
                        // Throw to trigger retry on server errors
                        if (response.status >= 500 || response.status === 429) {
                            throw new Error(`Server status ${response.status}`);
                        }
                        // Stop retrying on client errors (400, 401, etc)
                        console.warn(`[Translation] Non-retryable error for chunk: ${response.status}`);
                        break;
                    }

                    const data = await response.json();

                    if (data.data && data.data.translations) {
                        data.data.translations.forEach((t, index) => {
                            this.setCache(chunk[index], t.translatedText);
                        });
                        success = true;
                    }
                } catch (e) {
                    console.error(`[Translation] Chunk failed (Attempt ${attempts}):`, e);
                    if (attempts < 3) {
                        // Exponential backoff: 1s, 2s...
                        const delay = Math.pow(2, attempts) * 1000;
                        await new Promise(r => setTimeout(r, delay));
                    }
                }
            }
        }
        this.saveCache();
    }

    getCached(text) {
        return this.cache[this.currentLang]?.[text];
    }

    setCache(original, translated) {
        if (!this.cache[this.currentLang]) this.cache[this.currentLang] = {};
        this.cache[this.currentLang][original] = translated;
    }

    saveCache() {
        try {
            // Check quota before saving
            const serial = JSON.stringify(this.cache);
            localStorage.setItem('trans_cache', serial);
        } catch (e) {
            console.warn('Cache quota exceeded, pruning old cache...');
            // Simple pruning: keep only current language or clear all
            this.cache = {};
            if (this.currentLang !== 'en') {
                // Try to keep at least current lang? For now simple wipe is safer to restore functionality
                // Ideally we would delete keys not equal to currentLang
            }
        }
    }

    async translateText(text) {
        if (!text || !text.trim()) return text;
        if (this.currentLang === 'en') return text;

        // Check cache
        const cached = this.getCached(text);
        if (cached) return cached;

        // Fetch
        try {
            await this.fetchTranslations([text]);
            return this.getCached(text) || text;
        } catch (e) {
            console.error('Single text translation failed:', e);
            return text;
        }
    }

    /**
     * Watch for new DOM elements (Dynamic Content)
     */
    startObserver() {
        if (this.observer) return;

        this.observer = new MutationObserver(mutations => {
            let shouldTranslate = false;
            mutations.forEach(m => {
                if (m.addedNodes.length > 0) shouldTranslate = true;
                if (m.type === 'characterData') shouldTranslate = true;
            });

            if (shouldTranslate) {
                // Debounce simple
                clearTimeout(this.obsTimeout);
                this.obsTimeout = setTimeout(() => this.translatePage(), 500); // Faster reaction
            }
        });

        this.observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['placeholder', 'title', 'alt', 'aria-label', 'value'],
            characterData: true // needed for specific text node updates
        });
    }
}

// Initialize on Load
const globalTranslator = new TranslationService();

// Shim for legacy calls (Prevents crashes in Dashboard.cshtml before refactor)
// Shim for legacy calls (Prevents crashes in Dashboard.cshtml before refactor)
window.i18n = {
    t: (key) => {
        // Simple "CamelCase" -> "Camel Case" for readability
        return key ? key.replace(/([A-Z])/g, ' $1').trim() : '';
    },
    locale: localStorage.getItem('app_lang') || 'en'
};
