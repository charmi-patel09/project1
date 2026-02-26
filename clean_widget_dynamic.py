import os
import re

css_file = r'd:\\Antigravity\\project1\\wwwroot\\css\\dashboard.css'

with open(css_file, 'r', encoding='utf-8') as f:
    text = f.read()

# I need to clean up the duplicates created by multiple script runs earlier.
text = re.sub(r'\s*--widget-box-bg:\s*#[0-9A-Fa-f]+;', '', text)

# Safely inject inside root blocks exactly once
text = text.replace(':root {\n', ':root {\n    --widget-box-bg: #eaf4ff;\n')
text = text.replace('[data-time-theme="morning"] {\n', '[data-time-theme="morning"] {\n    --widget-box-bg: #eaf4ff;\n')
text = text.replace('[data-time-theme="noon"] {\n', '[data-time-theme="noon"] {\n    --widget-box-bg: #ffffff;\n')
text = text.replace('[data-time-theme="sunset"] {\n', '[data-time-theme="sunset"] {\n    --widget-box-bg: #ffe8dc;\n')
text = text.replace('[data-time-theme="night"] {\n', '[data-time-theme="night"] {\n    --widget-box-bg: #1f2a40;\n')

with open(css_file, 'w', encoding='utf-8') as f:
    f.write(text)

print('Cleaned up duplications successfully.')
