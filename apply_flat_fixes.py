import os
import re

file_paths = [
    r'd:\Antigravity\project1\wwwroot\css\dashboard.css',
    r'd:\Antigravity\project1\wwwroot\css\site.css'
]

for file_path in file_paths:
    with open(file_path, "r", encoding='utf-8') as f:
        content = f.read()

    # Replaces flat background inputs with gradient
    content = content.replace("background: #ffffff;", "background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;")

    with open(file_path, "w", encoding='utf-8') as f:
        f.write(content)

print("Removed more #ffffff flat backgrounds")
