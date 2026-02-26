import os
import re
import glob

views_files = glob.glob(r'd:\Antigravity\project1\Views\**\*.cshtml', recursive=True)

# Regex to match card backgrounds inside .cshtml files
pattern = re.compile(r'background:\s*(var\(--(?:bg-card|bg-surface|card-bg)\));')
pattern2 = re.compile(r'background-color:\s*(var\(--(?:bg-card|bg-surface|card-bg)\));')

for fpath in views_files:
    with open(fpath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content = pattern.sub(r'background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;', content)
    new_content = pattern2.sub(r'background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;', new_content)

    new_content = new_content.replace('background: var(--card-bg);', 'background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;')

    # Fix the TimeTracker Index.cshtml internal styles and inline
    new_content = new_content.replace('bg-card', 'bg-transparent border-0')
    new_content = new_content.replace('bg-transparent border-0 border-0', 'bg-transparent border-0')

    new_content = new_content.replace('background-color: var(--card-bg)', 'background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important')

    with open(fpath, 'w', encoding='utf-8') as f:
        f.write(new_content)

print('Replaced inline backgrounds in Views')
