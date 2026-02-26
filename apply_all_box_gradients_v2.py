import os
import re
import glob

css_files = [
    r'd:\Antigravity\project1\wwwroot\css\dashboard.css',
    r'd:\Antigravity\project1\wwwroot\css\site.css',
    r'd:\Antigravity\project1\wwwroot\css\currency-exchange.css'
]

# The gradient to apply
target_gradient = "linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important"

# Patterns to replace (common hardcoded backgrounds in card-like elements)
replace_patterns = [
    r"background-color:\s*(?:#fff(?:fff)?|white|rgba\(255,\s*255,\s*255,\s*0\.[0-9]+\))\s*;?",
    r"background:\s*(?:#fff(?:fff)?|white|rgba\(255,\s*255,\s*255,\s*0\.[0-9]+\))\s*;?",
    r"background-color:\s*var\(--(?:bg-card|bg-surface|card-bg|widget-box-bg|bg-elevated|bg-shell)\)\s*;?",
    r"background:\s*var\(--(?:bg-card|bg-surface|card-bg|widget-box-bg|bg-elevated|bg-shell)\)\s*;?"
]

# Specific classes mentioned by user or found in files
target_selectors = [
    ".card", ".bg-card", ".table-card", ".form-card", ".glass-panel", ".tool-card", ".modern-card", 
    ".stat-card", ".result-card", ".habit-item", ".emergency-number-card", ".goal-item-card", 
    ".day-card", ".auth-utility-box", ".widget-section-card", ".exchange-card", ".chrono-hub", 
    ".currency-hub", ".weather-hub", ".options-list", ".searchable-select-display", ".modern-select", 
    ".modern-input", ".search-field", ".hub-display", ".clock-display", ".zone-sync", ".btn-utility", 
    ".lang-dropdown-menu", "section", ".container", ".main-area", ".content-wrapper"
]

for fpath in css_files:
    if not os.path.exists(fpath):
        continue
    with open(fpath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 1. Broad replacement of hardcoded whites/vars in backgrounds
    for pattern in replace_patterns:
        content = re.sub(pattern, f"background: {target_gradient};", content)
    
    # 2. Add a global block at the end if it's site.css or dashboard.css to ensure these classes are covered
    if "site.css" in fpath or "dashboard.css" in fpath:
        extra_block = f"\n\n/* BROAD THEME BOX OVERRIDES */\n"
        extra_block += ", ".join(target_selectors) + " {\n"
        extra_block += f"    background: {target_gradient};\n"
        extra_block += "    border-color: var(--border-primary) !important;\n"
        extra_block += "}\n"
        content += extra_block

    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(content)

print("Broadly applied the User List gradient to all identified boxes and containers.")
