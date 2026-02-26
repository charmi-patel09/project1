import os
import re

base_dir = r'd:\\Antigravity\\project1\\Views'

count_inline = 0
for root, dirs, files in os.walk(base_dir):
    for f in files:
        if f.endswith('.cshtml'):
            path = os.path.join(root, f)
            with open(path, 'r', encoding='utf-8') as file:
                text = file.read()
                matches = re.findall(r'style="[^"]*(?:color\s*:|background\s*:|border(?:-color)?\s*:)[^"]*"', text)
                count_inline += len(matches)

print(f'Total HTML elements with inline color styles: {count_inline}')
