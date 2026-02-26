import os

files = [
    r'd:\Antigravity\project1\wwwroot\css\dashboard.css',
    r'd:\Antigravity\project1\wwwroot\css\site.css'
]

for file_path in files:
    with open(file_path, "r", encoding='utf-8') as f:
        content = f.read()
    
    # Replace var(--widget-box-bg) usages
    content = content.replace("background-color: var(--widget-box-bg) !important;", "background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;")
    content = content.replace("background-color: var(--widget-box-bg);", "background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%);")
    
    # Also replace it if it's referenced in night theme globals
    content = content.replace("background-color: var(--widget-box-bg) !important;", "background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;")
    
    # For night theme where it was hardcoded to #1e293b
    content = content.replace("background-color: #1e293b !important;", "background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-card) 100%) !important;")
    
    with open(file_path, "w", encoding='utf-8') as f:
        f.write(content)

print("Globally replaced box backgrounds with dynamic linear gradient.")
