import sys
import re

files = [
    r'd:\Antigravity\project1\Views\RoleWidgets\Index.cshtml',
    r'd:\Antigravity\project1\Views\Students\_UserForm.cshtml',
]

for file_path in files:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Strip body backrounds from style blocks
    content = re.sub(r'body\s*\{\s*background:\s*radial-gradient.*?\s*min-height: 100vh;\s*\}', '', content, flags=re.DOTALL)
    
    # Strip interfering class names
    content = content.replace('text-white', '')
    content = content.replace('bg-white', '')
    content = content.replace('bg-light', '')
    content = content.replace('bg-transparent', '')
    content = content.replace('text-dark', '')
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

print("Cleaned hardcoded UI classes from role widgets & user form.")
