import sys

file_path = r'd:\Antigravity\project1\wwwroot\css\site.css'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

old_block = """.card,
.bg-card,
.table-card,
.form-card,
.glass-panel,
.tool-card,
.modern-card,
.stat-card {"""

new_block = """.card,
.bg-card,
.table-card,
.form-card,
.glass-panel,
.tool-card,
.modern-card,
.stat-card,
.result-card,
.habit-item,
.emergency-number-card,
.goal-item-card,
.day-card,
.auth-utility-box,
.widget-section-card {"""

content = content.replace(old_block, new_block)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Expanded the global card linear gradient block to include all known sub-cards")
