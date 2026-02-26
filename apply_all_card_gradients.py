import os
import re
import glob

css_files = glob.glob(r'd:\Antigravity\project1\wwwroot\css\*.css')

# Regex to match background or background-color with any of the card-like var definitions
pattern = re.compile(r'background(?:-color)?:\s*(var\(--(?:bg-card|bg-surface|card-bg|bg-elevated|bg-shell)\))( !important)?\s*;')

for fpath in css_files:
    with open(fpath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Replace all matches with the linear gradient
    new_content = pattern.sub(r'background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;', content)
    
    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(new_content)

print("Replaced all dashboard and site CSS card backgrounds with linear gradient")
