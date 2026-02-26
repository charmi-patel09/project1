import os
import re

file_paths = [
    r'd:\Antigravity\project1\wwwroot\css\dashboard.css',
    r'd:\Antigravity\project1\wwwroot\css\site.css'
]

for file_path in file_paths:
    with open(file_path, "r", encoding='utf-8') as f:
        content = f.read()

    # Replaces flat background vars in theme blocks
    content = content.replace("--widget-box-bg: #ffffff;", "--widget-box-bg: var(--bg-secondary);")
    content = content.replace("--bg-surface: rgba(255, 255, 255, 0.95);", "--bg-surface: var(--bg-card);")
    
    # Noon theme card bg fix
    content = content.replace("--bg-card: rgba(255, 255, 255, 0.85);", "--bg-card: var(--card-bg);")

    with open(file_path, "w", encoding='utf-8') as f:
        f.write(content)

print("Removed more #ffffff flat backgrounds in theme blocks")
