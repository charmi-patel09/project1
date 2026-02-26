import os

css_file = r'd:\\Antigravity\\project1\\wwwroot\\css\\dashboard.css'

with open(css_file, 'a', encoding='utf-8') as f:
    f.write('''\n
/* User enforced widget dynamic theme styling */
:root, [data-time-theme="morning"], [data-time-theme="noon"], [data-time-theme="sunset"], [data-time-theme="night"] {
    --accent-color: var(--border-accent);
}

.chrono-hub,
.currency-hub,
.weather-hub,
.global-search-hub {
    background-color: var(--primary-color) !important;
    color: var(--text-color) !important;
    border-color: var(--accent-color) !important;
}

.chrono-hub *,
.currency-hub *,
.weather-hub *,
.global-search-hub * {
    color: inherit; /* Ensure inner elements adopt the forced text-color */
}
''')

print('Widget overrides applied successfully to dashboard.css')
