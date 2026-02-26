import os
import re

css_file1 = r'd:\\Antigravity\\project1\\wwwroot\\css\\dashboard.css'
css_file2 = r'd:\\Antigravity\\project1\\wwwroot\\css\\site.css'

with open(css_file1, 'a', encoding='utf-8') as f:
    f.write('''\n
/* GLOBAL NIGHT THEME FORM & UI OVERRIDES */
[data-time-theme="night"] input,
[data-time-theme="night"] select,
[data-time-theme="night"] textarea,
[data-time-theme="night"] .form-control,
[data-time-theme="night"] .form-select,
[data-time-theme="night"] .btn-utility,
[data-time-theme="night"] .lang-dropdown-menu,
[data-time-theme="night"] option {
    background-color: var(--widget-box-bg) !important;
    color: var(--text-color) !important;
    border-color: var(--border-accent) !important;
}

[data-time-theme="night"] ::placeholder {
    color: var(--text-dim) !important;
    opacity: 1 !important;
}
''')

with open(css_file2, 'a', encoding='utf-8') as f:
    f.write('''\n
/* GLOBAL NIGHT THEME FORM & UI OVERRIDES */
[data-time-theme="night"] input,
[data-time-theme="night"] select,
[data-time-theme="night"] textarea,
[data-time-theme="night"] .form-control,
[data-time-theme="night"] .form-select,
[data-time-theme="night"] .btn-utility,
[data-time-theme="night"] option {
    background-color: #1e293b !important;
    color: #f1f5f9 !important;
    border-color: #273C75 !important;
}

[data-time-theme="night"] ::placeholder {
    color: #94a3b8 !important;
    opacity: 1 !important;
}
''')

print('Applied global night theme overrides.')
