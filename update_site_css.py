import os
import re

css_file = r'd:\\Antigravity\\project1\\wwwroot\\css\\site.css'

with open(css_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace variables in each theme block in site.css
content = re.sub(r'(\[data-time-theme="morning"\] \{\s+--bg-color: var\(--bg-primary\);\s+)--card-bg: var\(--bg-card\);',
                 r'\1--card-bg: rgba(255, 255, 255, 0.85);\n    --bg-card: rgba(255, 255, 255, 0.85);', content)

content = re.sub(r'(\[data-time-theme="noon"\] \{\s+--bg-color: var\(--bg-primary\);\s+)--card-bg: var\(--bg-card\);',
                 r'\1--card-bg: rgba(255, 255, 255, 0.85);\n    --bg-card: rgba(255, 255, 255, 0.85);', content)

content = re.sub(r'(\[data-time-theme="sunset"\] \{\s+--bg-color: var\(--bg-primary\);\s+)--card-bg: var\(--bg-card\);',
                 r'\1--card-bg: rgba(255, 255, 255, 0.7);\n    --bg-card: rgba(255, 255, 255, 0.7);', content)

content = re.sub(r'(\[data-time-theme="night"\] \{\s+--bg-color: var\(--bg-primary\);\s+)--card-bg: var\(--bg-card\);',
                 r'\1--card-bg: rgba(16, 19, 42, 0.7);\n    --bg-card: rgba(16, 19, 42, 0.7);', content)

with open(css_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated site.css var(--bg-card) mappings")
