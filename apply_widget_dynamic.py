import sys
import re

css_file = r'd:\\Antigravity\\project1\\wwwroot\\css\\dashboard.css'

with open(css_file, 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace(':root {\n', ':root {\n    --widget-box-bg: #eaf4ff;\n')
text = text.replace('[data-time-theme="morning"] {\n', '[data-time-theme="morning"] {\n    --widget-box-bg: #eaf4ff;\n')
text = text.replace('[data-time-theme="noon"] {\n', '[data-time-theme="noon"] {\n    --widget-box-bg: #ffffff;\n')
text = text.replace('[data-time-theme="sunset"] {\n', '[data-time-theme="sunset"] {\n    --widget-box-bg: #ffe8dc;\n')
text = text.replace('[data-time-theme="night"] {\n', '[data-time-theme="night"] {\n    --widget-box-bg: #1f2a40;\n')

# Check if start marker exists, if so cut string
start_marker = "/* User enforced widget dynamic theme styling */"
if start_marker in text:
    text = text[:text.index(start_marker)]

# Make sure we don't duplicate
if '--- WIDGET DYNAMIC BACKGROUND OVERRIDES ---' in text:
    text = text[:text.index('/* --- WIDGET DYNAMIC BACKGROUND OVERRIDES --- */')]

text = text.strip() + '''\n
/* --- WIDGET DYNAMIC BACKGROUND OVERRIDES --- */

/* Parent Widget Cards Background */
.chrono-hub,
.currency-hub,
.weather-hub {
    background-color: var(--widget-box-bg) !important;
}

/* Inner elements: Chronos Sync box, Currency From/To boxes, Weather location box */
.chrono-hub .searchable-select-display,
.chrono-hub .searchable-select-dropdown,
.chrono-hub .options-list,
.chrono-hub .clock-display,
.chrono-hub .zone-sync,
.chrono-hub .modern-select,

.currency-hub .searchable-select-display,
.currency-hub .searchable-select-dropdown,
.currency-hub .options-list,
.currency-hub .modern-input,
.currency-hub .modern-select,

.weather-hub .search-field,
.weather-hub .search-field input,
.weather-hub .primary-result,
.weather-hub .hub-display {
    background-color: var(--widget-box-bg) !important;
    color: var(--text-color) !important;
    border-color: var(--accent-primary) !important;
    transition: background-color 0.4s ease, color 0.4s ease, border-color 0.4s ease;
}

/* All select/dropdown fields inside these widgets */
.chrono-hub select,
.currency-hub select,
.weather-hub select,
.chrono-hub input,
.currency-hub input,
.weather-hub input,
.chrono-hub .search-input-wrapper input,
.currency-hub .search-input-wrapper input {
    background-color: var(--widget-box-bg) !important;
    color: var(--text-color) !important;
}

/* Force inherit color downward to prevent clashes */
.chrono-hub *,
.currency-hub *,
.weather-hub * {
    color: inherit;
}
'''
with open(css_file, 'w', encoding='utf-8') as f:
    f.write(text)

print('Done')
