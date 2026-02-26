import os
import glob

views_dir = r"d:\Antigravity\project1\Views\Students\*\*"

files = glob.glob(r"d:\Antigravity\project1\Views\Students\*.cshtml")
for file_path in files:
    with open(file_path, "r", encoding='utf-8') as f:
        content = f.read()
    
    # Remove bg-light, bg-white
    content = content.replace("bg-light", "bg-card")
    content = content.replace("bg-white", "bg-card")
    content = content.replace("text-dark", "text-primary")
    
    with open(file_path, "w", encoding='utf-8') as f:
        f.write(content)

print('Cleaned Students views')
