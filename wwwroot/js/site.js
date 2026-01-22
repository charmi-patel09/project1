/**
 * Toggles the language dropdown visibility
 */
function toggleLangDropdown() {
    const dropdown = document.getElementById('langDropdown');
    if (dropdown) {
        if (dropdown.style.display === 'none' || !dropdown.style.display) {
            dropdown.style.display = 'block';
        } else {
            dropdown.style.display = 'none';
        }
    }
    // jQuery fallback if loaded (for fade animation compatibility if expected)
    if (window.jQuery) $('#langDropdown').fadeToggle(200);
}

// Add click-outside listener for dropdown
document.addEventListener('click', function (e) {
    const selector = document.querySelector('.language-selector');
    const dropdown = document.getElementById('langDropdown');

    if (selector && dropdown && !selector.contains(e.target)) {
        dropdown.style.display = 'none';
    }
});

// SYNDASH ADMIN DASHBOARD - JAVASCRIPT
// ============================================

/**
 * Global ISO Language Registry
 * Shared across MVC Views and Static Pages
 */
const ISOLanguages = {
    'en': 'English',
    'hi': 'हिन्दी (Hindi)',
    'gu': 'ગુજરાતી (Gujarati)',
    'mr': 'मराठी (Marathi)',
    'fr': 'Français (French)',
    'es': 'Español (Spanish)',
    'de': 'Deutsch (German)',
    'ar': 'العربية (Arabic)',
    'zh': '中文 (Chinese)',
    'ja': '日本語 (Japanese)',
    'ru': 'Русский (Russian)',
    'pt': 'Português (Portuguese)',
    'it': 'Italiano (Italian)',
    'ko': '한국어 (Korean)',
    'nl': 'Nederlands (Dutch)',
    'tr': 'Türkçe (Turkish)',
    'pl': 'Polski (Polish)',
    'id': 'Bahasa Indonesia',
    'th': 'ไทย (Thai)',
    'vi': 'Tiếng Việt (Vietnamese)',
    'uk': 'Українська (Ukrainian)',
    'sv': 'Svenska (Swedish)',
    'fi': 'Suomi (Finnish)',
    'da': 'Dansk (Danish)',
    'no': 'Norsk (Norwegian)',
    'cs': 'Čeština (Czech)',
    'el': 'Ελληνικά (Greek)',
    'he': 'עברית (Hebrew)',
    'ro': 'Română (Romanian)',
    'hu': 'Magyar (Hungarian)'
};

// Global Translation Engine State
window.i18n = {
    locale: 'en',
    translations: {},
    t: function (key) {
        return this.translations[key] || key; // Simple Key Fallback
    }
};

document.addEventListener('DOMContentLoaded', function () {
    // 1. Sidebar Toggle & Generic UI
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sidebar');
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', () => sidebar.classList.toggle('active'));
        document.addEventListener('click', (event) => {
            if (window.innerWidth <= 768 && !sidebar.contains(event.target) && !sidebarToggle.contains(event.target)) {
                sidebar.classList.remove('active');
            }
        });
    }

    // 2. Active Link Highlighting
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link').forEach(link => {
        const linkPath = link.getAttribute('href');
        if (linkPath && currentPath.includes(linkPath) && linkPath !== '/') link.classList.add('active');
        else if (linkPath === '/' && currentPath === '/') link.classList.add('active');
    });

    // 3. Auto-Initialize Localization
    initLocalization();
});

// ============================================
// COMPATIBILITY SHIM FOR NEW API TRANSLATION
// ============================================

// Legacy i18n object to prevent crashes in existing widgets
window.i18n = {
    locale: localStorage.getItem('app_lang') || 'en',
    t: function (key) {
        // Return key (or mapped English text) so it renders in DOM
        // The TranslationService will then translate this text via API
        return key.replace(/([A-Z])/g, ' $1').trim(); // "NewNote" -> "New Note"
    }
};

// Hook for legacy init calls
function initLocalization() {
    console.log('[Compatibility] Legacy localization init called. Deferring to TranslationService.');
}

// Redirect legacy language setters to new system
async function setLanguage(lang) {
    if (window.translator) {
        await window.translator.setLanguage(lang);
    } else {
        localStorage.setItem('app_lang', lang);
        location.reload();
    }
}

// ============================================

/**
 * Updates all static UI elements with [data-i18n] attributes
 */
function updateGlobalUI() {
    const t = window.i18n.t.bind(window.i18n);

    // Text Content
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.innerText = t(key);
    });

    // Placeholders
    document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
        const key = el.getAttribute('data-i18n-placeholder');
        el.setAttribute('placeholder', t(key));
    });

    // Tooltips / Titles
    document.querySelectorAll('[data-i18n-title]').forEach(el => {
        const key = el.getAttribute('data-i18n-title');
        el.setAttribute('title', t(key));
    });

    // Alt Text
    document.querySelectorAll('[data-i18n-alt]').forEach(el => {
        const key = el.getAttribute('data-i18n-alt');
        el.setAttribute('alt', t(key));
    });

    // Values (Buttons)
    document.querySelectorAll('[data-i18n-value]').forEach(el => {
        const key = el.getAttribute('data-i18n-value');
        el.value = t(key);
    });
}

/**
 * Populates language dropdowns dynamically
 */
function populateLanguageDropdowns() {
    // 1. Layout Dropdown (Ul/Div style)
    const list = document.getElementById('langList');
    if (list) {
        let html = '';
        Object.keys(ISOLanguages).forEach(code => {
            html += `<a class="lang-item" onclick="setLanguage('${code}')">${ISOLanguages[code]}</a>`;
        });
        list.innerHTML = html;

        // Search Filter
        const searchBox = document.getElementById('langSearch');
        if (searchBox) {
            searchBox.addEventListener('input', (e) => {
                const term = e.target.value.toLowerCase();
                let html = '';
                Object.keys(ISOLanguages).forEach(code => {
                    const name = ISOLanguages[code];
                    if (name.toLowerCase().includes(term) || code.includes(term)) {
                        html += `<a class="lang-item" onclick="setLanguage('${code}')">${name}</a>`;
                    }
                });
                list.innerHTML = html || '<div class="lang-item" style="color:#888">No results</div>';
            });
        }
    }

    // 2. Standalone Select (Currency Page)
    const select = document.getElementById('lang-select');
    if (select) {
        select.innerHTML = '';
        Object.keys(ISOLanguages).forEach(code => {
            const opt = document.createElement('option');
            opt.value = code;
            opt.innerText = ISOLanguages[code];
            select.appendChild(opt);
        });
        select.value = window.i18n.locale;
    }
}

/**
 * Maps an ISO language code (e.g., 'en', 'fr', 'ja') to Google News parameters.
 */
function getGoogleNewsParams(lang) {
    // Default map for specific overrides
    const overrides = {
        'en': { gl: 'US', hl: 'en-US', ceid: 'US:en' },
        'hi': { gl: 'IN', hl: 'hi', ceid: 'IN:hi' },
        'gu': { gl: 'IN', hl: 'gu', ceid: 'IN:gu' },
        'mr': { gl: 'IN', hl: 'mr', ceid: 'IN:mr' },
        'zh': { gl: 'CN', hl: 'zh-CN', ceid: 'CN:zh-Hans' },
        'ja': { gl: 'JP', hl: 'ja', ceid: 'JP:ja' },
        'ko': { gl: 'KR', hl: 'ko', ceid: 'KR:ko' },
        'uk': { gl: 'UA', hl: 'uk', ceid: 'UA:uk' },
        'pt': { gl: 'BR', hl: 'pt-BR', ceid: 'BR:pt-419' }
    };

    if (overrides[lang]) return overrides[lang];

    const regionMap = {
        'fr': 'FR', 'es': 'MX', 'de': 'DE', 'ar': 'EG', 'ru': 'RU', 'it': 'IT',
        'nl': 'NL', 'tr': 'TR', 'pl': 'PL', 'id': 'ID', 'th': 'TH', 'vi': 'VN',
        'sv': 'SE', 'fi': 'FI', 'da': 'DK', 'no': 'NO', 'cs': 'CZ', 'el': 'GR',
        'he': 'IL', 'ro': 'RO', 'hu': 'HU'
    };

    const region = regionMap[lang] || 'US';
    const hl = lang;
    return { gl: region, hl: hl, ceid: `${region}:${hl}` };
}
