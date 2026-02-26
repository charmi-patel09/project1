import os
import glob

views_files = glob.glob(r'd:\Antigravity\project1\Views\**\*.cshtml', recursive=True)

for fpath in views_files:
    with open(fpath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Repair corrupted var(--bg-card)
    content = content.replace('var(--bg-transparent border-0)', 'var(--bg-card)')
    content = content.replace('var(--transparent border-0)', 'var(--card-bg)')

    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(content)

print('Repaired corrupted CSS variables')
