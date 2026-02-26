'
import os
css_file = r"d:\\Antigravity\\project1\\wwwroot\\css\\site.css"

with open(css_file, "a", encoding="utf-8") as f:
    f.write("""

/* GLOBAL CARD THEME OVERRIDES */
.card,
.bg-card,
.table-card,
.form-card,
.glass-panel {
    background-color: var(--bg-card) !important;
    border-color: var(--border-primary) !important;
    color: var(--text-primary) !important;
}

.card-header,
.card-footer {
    background-color: rgba(0,0,0,0.05) !important;
    border-color: var(--border-primary) !important;
}

[data-time-theme="night"] .card-header,
[data-time-theme="night"] .card-footer {
    background-color: var(--bg-tertiary) !important;
}

[data-time-theme="morning"] .text-white,
[data-time-theme="noon"] .text-white {
    color: var(--text-primary) !important;
}
""")
print("Added global card overrides.")
'
