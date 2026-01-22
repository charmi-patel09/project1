// ==================== Localization System ====================
const Localization = {
    locale: 'en',
    translations: {},

    async init() {
        // 1. Resolve Language: URL param > localStorage > Browser > Default
        const urlParams = new URLSearchParams(window.location.search);
        let lang = urlParams.get('lang') || localStorage.getItem('user_lang') || navigator.language.substring(0, 2);

        // Supported fallback
        if (typeof ISOLanguages !== 'undefined' && !ISOLanguages[lang]) {
            lang = 'en';
        }

        await this.setLanguage(lang);

        // Listen for storage changes (multi-tab sync)
        window.addEventListener('storage', (e) => {
            if (e.key === 'user_lang' && e.newValue !== this.locale) {
                this.setLanguage(e.newValue);
                // Update select if exists
                const sel = document.getElementById('lang-select');
                if (sel) sel.value = e.newValue;
            }
        });
    },

    async setLanguage(lang) {
        if (!lang) lang = 'en';

        try {
            // Load JSON
            const res = await fetch(`/locales/${lang}.json?v=${new Date().getTime()}`);
            if (res.ok) {
                this.translations = await res.json();
                this.locale = lang;

                // Persist
                localStorage.setItem('user_lang', lang);
                // Cookie for ASP.NET Core compatibility
                document.cookie = `.AspNetCore.Culture=c=${lang}|uic=${lang};path=/;max-age=31536000`;

                // Update UI
                this.updateUI();

                // Update HTML lang attribute
                document.documentElement.lang = lang;
            } else {
                console.warn(`Locale ${lang} not found, falling back to en`);
                if (lang !== 'en') await this.setLanguage('en');
            }
        } catch (e) {
            console.error('Localization Load Error:', e);
        }
    },

    updateUI() {
        // Text Content
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            if (this.translations[key]) {
                el.textContent = this.translations[key];
            }
        });

        // Placeholders
        document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
            const key = el.getAttribute('data-i18n-placeholder');
            if (this.translations[key]) {
                el.placeholder = this.translations[key];
            }
        });

        // Update Dynamic Elements if needed
        if (typeof updateDynamicDate === 'function') updateDynamicDate();
    },

    t(key) {
        return this.translations[key] || key;
    }
};
