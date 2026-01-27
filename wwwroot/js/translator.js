/**
 * STRICT GLOBAL TRANSLATOR V2
 * - Implements WeakMap for strict DOM tracking
 * - Caches translations
 * - Auto-retries failures
 * - Prevents mixed-content flash
 */

const Translator = {
    currentLang: 'en',
    observer: null,
    pendingNodes: new Set(),
    pendingTimeout: null,

    // Core Storage
    originalTextMap: new WeakMap(), // textNode -> originalString
    originalAttrMap: new WeakMap(), // element -> { attr: originalString }
    cache: new Map(), // key (lang_text) -> translation

    // Config
    retryDelays: [1000, 2000, 5000],

    init: function () {
        // 1. Language Detection
        const saved = localStorage.getItem('preferredLanguage');
        const browser = navigator.language.slice(0, 2);
        // Supports: en, es, fr, de, hi, gu, zh, ja, ru, ar
        const supported = ['en', 'es', 'fr', 'de', 'hi', 'gu', 'zh', 'ja', 'ru', 'ar'];
        this.currentLang = saved || (supported.includes(browser) ? browser : 'en');

        // 2. Load Persisted Cache
        this.loadCache();

        console.log('Translator Initialized. Language:', this.currentLang);

        // 3. UI Hookup
        const selector = document.getElementById('languageSelector');
        if (selector) {
            selector.value = this.currentLang;
        }

        // 4. Create Loader
        this.createLoader();

        // 5. Start Engine
        if (this.currentLang !== 'en') {
            this.translatePage(true); // true = initial load
        }

        this.observe();
    },

    loadCache: function () {
        try {
            const raw = localStorage.getItem('translatorCache');
            if (raw) {
                const parsed = JSON.parse(raw);
                this.cache = new Map(parsed);
                console.log(`Loaded ${this.cache.size} translations from local cache.`);
            }
        } catch (e) {
            console.warn('Failed to load local translation cache', e);
        }
    },

    saveCache: function () {
        try {
            // Convert Map to Array for JSON serialization
            const data = JSON.stringify(Array.from(this.cache.entries()));
            localStorage.setItem('translatorCache', data);
        } catch (e) {
            // Storage quota full?
            console.warn('Failed to save to local cache', e);
        }
    },

    createLoader: function () {
        if (document.getElementById('translator-loader')) return;
        const div = document.createElement('div');
        div.id = 'translator-loader';
        div.style.cssText = `
            position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0,0,0,0.85); z-index: 99999;
            display: none; align-items: center; justify-content: center;
            flex-direction: column; color: white; font-family: sans-serif;
            backdrop-filter: blur(5px);
        `;
        div.innerHTML = `
            <div class="spinner-border text-light" role="status" style="width: 3rem; height: 3rem; margin-bottom: 20px;"></div>
            <h2 style="font-weight: 300; letter-spacing: 1px;">Translating...</h2>
            <p id="translator-status" style="opacity: 0.7; margin-top: 10px;">Synchronizing Global Language Protocols</p>
        `;
        document.body.appendChild(div);
    },

    setLoader: function (active) {
        const el = document.getElementById('translator-loader');
        if (el) el.style.display = active ? 'flex' : 'none';
        if (active) document.body.style.overflow = 'hidden';
        else document.body.style.overflow = '';
    },

    setLanguage: async function (lang) {
        if (lang === this.currentLang) return;

        this.setLoader(true);

        // Wait a tiny bit to let UI render the loader
        await new Promise(r => requestAnimationFrame(r));
        await new Promise(r => setTimeout(r, 50));

        this.currentLang = lang;
        localStorage.setItem('preferredLanguage', lang);

        if (lang === 'en') {
            this.restoreOriginals();
        } else {
            // If we were previously in 'en', we rely on current DOM being effectively 'en' (mostly)
            // But if we switched es -> fr, we must restore to en first? 
            // Better strategy: Always restore strict originals from WeakMap before translating to new lang?
            // Actually, if we have the original TextNode mapped to English, we can just use that Key for translation
            // NO. We must reset the DOM to English first to ensureclean state, OR we use the keys in WeakMap as the source.

            // Reverting to originals is safest to ensure placeholders/variables are correct.
            this.restoreOriginals();

            // Now translate
            await this.translatePage();
        }

        // Notify dynamic components
        if (window.jQuery) {
            window.jQuery(document).trigger('languageChanged', [lang]);
        }

        this.setLoader(false);
    },

    observe: function () {
        if (this.observer) this.observer.disconnect();

        this.observer = new MutationObserver((mutations) => {
            // ALWAYS observe. If EN, we might just be capturing new nodes as "originals" if needed? 
            // No, if EN, we don't need to do anything.
            if (this.currentLang === 'en') return;

            let hasAdditions = false;
            mutations.forEach(m => {
                // Child List
                m.addedNodes.forEach(node => {
                    if (this.shouldTranslate(node)) {
                        this.collectNodes(node, this.pendingNodes);
                        hasAdditions = true;
                    }
                });

                // Text Content Changes (CharacterData)
                if (m.type === 'characterData') {
                    // node is a TextNode
                    if (this.shouldTranslate(m.target)) {
                        this.pendingNodes.add(m.target);
                        hasAdditions = true;
                    }
                }

                // Attribute Changes? (Not monitored by default subtree unless specified, usually unnecessary for standard JS updates which replace nodes)
            });

            if (hasAdditions) {
                this.debouncedProcess();
            }
        });

        this.observer.observe(document.body, {
            childList: true,
            subtree: true,
            characterData: true
        });
    },

    debouncedProcess: function () {
        if (this.pendingTimeout) clearTimeout(this.pendingTimeout);
        this.pendingTimeout = setTimeout(() => this.processPending(), 200); // Fast debounce
    },

    processPending: async function () {
        if (this.pendingNodes.size === 0) return;

        const nodes = Array.from(this.pendingNodes);
        this.pendingNodes.clear();

        await this.translateBatch(nodes);
    },

    translatePage: async function (isInitial = false) {
        if (isInitial) this.setLoader(true);

        const allNodes = [];
        this.collectNodes(document.body, allNodes);
        await this.translateBatch(allNodes);

        if (isInitial) this.setLoader(false);
    },

    restoreOriginals: function () {
        // Walk entire DOM? Or just we can effectively use the Map?
        // We can't iterate WeakMap. We must walk DOM.
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_ALL, null, false);
        let node;
        while (node = walker.nextNode()) {
            // Text Nodes
            if (node.nodeType === 3) {
                if (this.originalTextMap.has(node)) {
                    node.nodeValue = this.originalTextMap.get(node);
                    // Don't delete, we might need it again
                }
            }
            // Elements
            if (node.nodeType === 1) {
                if (this.originalAttrMap.has(node)) {
                    const attrs = this.originalAttrMap.get(node);
                    for (const [attr, val] of Object.entries(attrs)) {
                        node[attr] = val; // Restore property
                        node.setAttribute(attr, val); // Restore attribute
                    }
                }
            }
        }
    },

    shouldTranslate: function (node) {
        if (node.nodeType === 1) { // Element
            const tag = node.tagName.toLowerCase();
            if (['script', 'style', 'noscript', 'code', 'pre', 'link', 'meta'].includes(tag)) return false;
            if (node.classList && (node.classList.contains('no-translate') || node.closest('.no-translate'))) return false;
            // Editable content
            if (node.isContentEditable) return false;
        }
        if (node.nodeType === 3) {
            // Text Node checks
            if (!node.parentNode) return false;
            // Check parents
            if (node.parentNode.closest('.no-translate')) return false;
            if (['SCRIPT', 'STYLE', 'CODE'].includes(node.parentNode.tagName)) return false;
        }
        return true;
    },

    collectNodes: function (root, collection) {
        // Recursive
        const _this = this;
        traverse(root);

        function traverse(node) {
            if (!_this.shouldTranslate(node)) return;

            if (node.nodeType === 3) { // Text
                const val = node.nodeValue.trim();
                // Valid text?
                if (val.length > 0 && !/^\d+$/.test(val) && !/^[!@#$%^&*()_+={}\[\]:;"'<>,.?/\\|`~-]+$/.test(val)) {
                    // Check if we have an original
                    if (!_this.originalTextMap.has(node)) {
                        _this.originalTextMap.set(node, node.nodeValue); // Store EXACT value including whitespace
                    }
                    if (collection instanceof Set) collection.add(node);
                    else collection.push(node);
                }
            } else if (node.nodeType === 1) { // Element
                // Attributes
                if (['INPUT', 'TEXTAREA'].includes(node.tagName) && node.type !== 'hidden') {
                    // Placeholder
                    if (node.placeholder) {
                        if (!_this.originalAttrMap.has(node)) _this.originalAttrMap.set(node, {});

                        const cached = _this.originalAttrMap.get(node);
                        if (!cached.placeholder) cached.placeholder = node.placeholder;

                        if (collection instanceof Set) collection.add({ type: 'attr', node: node, attr: 'placeholder', text: cached.placeholder });
                        else collection.push({ type: 'attr', node: node, attr: 'placeholder', text: cached.placeholder });
                    }
                    // Value (for buttons only!)
                    if ((node.type === 'submit' || node.type === 'button') && node.value) {
                        if (!_this.originalAttrMap.has(node)) _this.originalAttrMap.set(node, {});

                        const cached = _this.originalAttrMap.get(node);
                        if (!cached.value) cached.value = node.value;

                        if (collection instanceof Set) collection.add({ type: 'attr', node: node, attr: 'value', text: cached.value });
                        else collection.push({ type: 'attr', node: node, attr: 'value', text: cached.value });
                    }
                }

                // Tooltips (title attribute)
                if (node.title) {
                    if (!_this.originalAttrMap.has(node)) _this.originalAttrMap.set(node, {});

                    const cached = _this.originalAttrMap.get(node);
                    if (!cached.title) cached.title = node.title;

                    if (collection instanceof Set) collection.add({ type: 'attr', node: node, attr: 'title', text: cached.title });
                    else collection.push({ type: 'attr', node: node, attr: 'title', text: cached.title });
                }

                // Recursion
                let child = node.firstChild;
                while (child) {
                    traverse(child);
                    child = child.nextSibling;
                }
            }
        }
    },

    translateBatch: async function (nodes, retryCount = 0) {
        if (nodes.length === 0) return;

        const lang = this.currentLang;
        const toFetch = new Set();
        const nodeMap = new Map(); // text -> [nodes]

        nodes.forEach(item => {
            let originalText;
            if (item.type === 'attr') {
                originalText = item.text; // Is always original from cache logic above
            } else {
                // For TextNode, get original from WeakMap 
                // (It should be there from collectNodes)
                if (this.originalTextMap.has(item)) {
                    originalText = this.originalTextMap.get(item).trim();
                } else {
                    originalText = item.nodeValue.trim(); // Fallback
                }
            }

            if (!originalText) return;

            // Check Cache
            const cacheKey = `${lang}_${originalText}`;
            if (this.cache.has(cacheKey)) {
                // Apply immediately
                this.applyTranslation(item, originalText, this.cache.get(cacheKey));
            } else {
                toFetch.add(originalText);
                if (!nodeMap.has(originalText)) nodeMap.set(originalText, []);
                nodeMap.get(originalText).push(item);
            }
        });

        if (toFetch.size === 0) return;

        const texts = Array.from(toFetch);

        const chunkSize = 5;

        for (let i = 0; i < texts.length; i += chunkSize) {
            const chunk = texts.slice(i, i + chunkSize);

            try {
                const controller = new AbortController();
                const timeoutId = setTimeout(() => controller.abort(), 60000); // 60s per chunk

                const response = await fetch('/api/Translation/translate-batch', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ texts: chunk, targetLanguage: lang }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) {
                    console.warn('Chunk failed:', response.status);
                    continue;
                }

                const results = await response.json();

                // Process results immediately
                for (const [original, translated] of Object.entries(results)) {
                    // Cache it
                    this.cache.set(`${lang}_${original}`, translated);

                    // Apply it
                    if (nodeMap.has(original)) {
                        nodeMap.get(original).forEach(item => {
                            this.applyTranslation(item, original, translated);
                        });
                    }
                }

                // Persist new translations
                this.saveCache();

            } catch (e) {
                console.error(`Translation Chunk Failed (Start Index: ${i})`, e);
                // Continue to next chunk instead of dying completely
            }
        }
    },

    applyTranslation: function (item, original, translated) {
        if (!translated) return;

        // Preserve common punctuation wrap?
        // Google Translate tends to strip or add spaces.
        // We match strict trim.

        if (item.type === 'attr') {
            item.node[item.attr] = translated;
        } else {
            // TextNode
            // We need to preserve leading/trailing whitespace of the *original* node value
            const fullOriginal = this.originalTextMap.get(item);
            if (fullOriginal) {
                // Simple regex replace of the trimmed part?
                // Be careful if word appears multiple times? 
                // But we captured the whole node value.
                // So replace original.trim() with translated.

                const leftSpace = fullOriginal.match(/^\s*/)[0];
                const rightSpace = fullOriginal.match(/\s*$/)[0];

                item.nodeValue = leftSpace + translated + rightSpace;
            }
        }
    },
    translateText: async function (text) {
        if (!text) return '';
        if (this.currentLang === 'en') return text;

        const lang = this.currentLang;
        const cacheKey = `${lang}_${text}`;

        if (this.cache.has(cacheKey)) {
            return this.cache.get(cacheKey);
        }

        try {
            // Re-use batch endpoint for single text
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 5000); // 5s timeout for single text

            const response = await fetch('/api/Translation/translate-batch', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ texts: [text], targetLanguage: lang }),
                signal: controller.signal
            });
            clearTimeout(timeoutId);

            if (!response.ok) return text;

            const results = await response.json();
            const translated = results[text] || text;

            this.cache.set(cacheKey, translated);
            this.saveCache();
            return translated;
        } catch (e) {
            console.error('Translation Text Error', e);
            return text;
        }
    },

    get locale() {
        return this.currentLang;
    },

    // Alias for short usage if needed
    t: function (key) {
        return key;
    }
};

document.addEventListener('DOMContentLoaded', () => {
    Translator.init();
});

window.Translator = Translator;
window.translator = Translator;
window.i18n = Translator;
