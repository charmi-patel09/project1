import os
import re

css_file = r'd:\\Antigravity\\project1\\wwwroot\\css\\dashboard.css'

with open(css_file, 'r', encoding='utf-8') as f:
    text = f.read()

# I will carefully inject --widget-box-bg into the appropriate definitions
text = re.sub(r'(:root\s*\{)', r'\1\n    --widget-box-bg: #ffffff;', text)
text = re.sub(r'(\[data-time-theme="morning"\]\s*\{)', r'\1\n    --widget-box-bg: #eaf4ff;', text)
text = re.sub(r'(\[data-time-theme="noon"\]\s*\{)', r'\1\n    --widget-box-bg: #ffffff;', text)
text = re.sub(r'(\[data-time-theme="sunset"\]\s*\{)', r'\1\n    --widget-box-bg: #ffe8dc;', text)
text = re.sub(r'(\[data-time-theme="night"\]\s*\{)', r'\1\n    --widget-box-bg: #1f2a40;', text)

# Remove that forceful block that I added in the previous step
text = re.sub(r'/\* User enforced widget dynamic theme styling \*/.*?(?=\Z|/\*)', '', text, flags=re.DOTALL)

# Now I'll append the rules addressing the specific components requested by user
with open(css_file, 'w', encoding='utf-8') as f:
    f.write(text.strip() + '''\n
/* --- WIDGET DYNAMIC BACKGROUND OVERRIDES --- */

/* Parent Widget Cards Background (User asked to update widgets to use dynamic theme-based box background) */
.tool-card.chrono-hub,
.tool-card.currency-hub,
.tool-card.weather-hub {
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
    transition: background-color 0.4s ease, color 0.4s ease;
}

/* Ensuring inner select/dropdown fields inside these widgets */
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

/* Clear text conflicts if inner elements nested inside */
.chrono-hub *,
.currency-hub *,
.weather-hub * {
    color: inherit;
}
''')
        
