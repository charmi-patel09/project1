import os
import re

file_path = r'd:\Antigravity\project1\wwwroot\css\site.css'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Define the clean theme blocks
root_block = """:root {
    --bg-color: var(--bg-primary);
    --card-bg: var(--bg-card);
    --primary-color: var(--accent-primary);
    --secondary-color: var(--bg-secondary);
    --text-color: var(--text-primary);
    --border-color: var(--border-primary);

    --bg-primary: #E17055;
    --bg-secondary: #D63031;
    --bg-tertiary: rgba(225, 112, 85, 0.1);

    --bg-sidebar-top: #E17055;
    --bg-sidebar-bottom: #D63031;
    --bg-sidebar: var(--bg-sidebar-top);

    --bg-topbar: #ffffff;
    --bg-card: rgba(255, 255, 255, 0.7);
    --bg-card-tint: rgba(225, 112, 85, 0.15);
    --bg-hover: rgba(225, 112, 85, 0.2);
    --bg-active: rgba(225, 112, 85, 0.3);

    --border-primary: rgba(45, 52, 54, 0.1);
    --border-secondary: rgba(45, 52, 54, 0.05);
    --border-accent: #E17055;

    --text-primary: #3A3A3A;
    --text-primary-rgb: 58, 58, 58;
    --text-secondary: #4A4A4A;
    --text-muted: #636e72;
    --text-heading: #2D3436;
    --text-accent: #6C5CE7;
    --text-white: #ffffff;
    --text-input: #3A3A3A;
    --text-select: #3A3A3A;

    --accent-primary: #E17055;
    --accent-primary-rgb: 225, 112, 85;
    --accent-hover: #D35400;
    --accent-success: #00b894;
    --accent-warning: #fdcb6e;
    --accent-danger: #d63031;
    --accent-purple: #6C5CE7;
    --accent-emerald: #55efc4;

    --shadow-sm: 0 4px 6px rgba(255, 94, 98, 0.1);
    --shadow-md: 0 8px 16px rgba(255, 94, 98, 0.15);
    --shadow-lg: 0 12px 24px rgba(255, 94, 98, 0.2);
    --shadow-xl: 0 20px 40px rgba(255, 94, 98, 0.3);
    --shadow-glow: 0 0 20px rgba(225, 112, 85, 0.3);

    --transition-fast: 0.15s ease;
    --transition-normal: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    --transition-slow: 0.5s cubic-bezier(0.4, 0, 0.2, 1);

    --sidebar-width: 280px;
    --sidebar-collapsed-width: 80px;
    --topbar-height: 70px;

    --font-primary: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    --font-heading: 'Plus Jakarta Sans', sans-serif;

    --text-sidebar-heading: var(--text-white);
    --text-sidebar-sub: rgba(255, 255, 255, 0.8);
    --text-sidebar-icon: var(--text-white);
}"""

morning_block = """[data-time-theme="morning"] {
    --bg-color: var(--bg-primary);
    --card-bg: rgba(255, 250, 245, 0.85);
    --bg-card: rgba(255, 250, 245, 0.85);
    --primary-color: var(--accent-primary);
    --secondary-color: var(--bg-secondary);
    --text-color: var(--text-primary);
    --border-color: var(--border-primary);

    --bg-primary: #FFF0E6;
    --bg-secondary: #FFE4D6;
    --bg-tertiary: rgba(255, 167, 38, 0.1);

    --bg-sidebar-top: #FFB74D;
    --bg-sidebar-bottom: #FF8A65;
    --bg-sidebar: var(--bg-sidebar-top);

    --bg-card-tint: rgba(255, 167, 38, 0.08);
    --bg-hover: rgba(255, 167, 38, 0.15);
    --bg-active: rgba(255, 167, 38, 0.25);

    --border-accent: #FFA726;

    --accent-primary: #FFA726;
    --accent-primary-rgb: 255, 167, 38;
    --accent-hover: #F57C00;

    --shadow-sm: 0 4px 6px rgba(255, 167, 38, 0.1);
    --shadow-md: 0 8px 16px rgba(255, 167, 38, 0.15);
    --shadow-lg: 0 12px 24px rgba(255, 167, 38, 0.2);
    --shadow-xl: 0 20px 40px rgba(255, 167, 38, 0.3);
    --shadow-glow: 0 0 20px rgba(255, 167, 38, 0.3);

    --text-primary-rgb: 58, 58, 58;
    --text-sidebar-heading: var(--text-white);
    --text-sidebar-sub: rgba(255, 255, 255, 0.8);
    --text-sidebar-icon: var(--text-white);
}"""

noon_block = """[data-time-theme="noon"] {
    --bg-color: var(--bg-primary);
    --card-bg: rgba(255, 255, 255, 0.95);
    --bg-card: rgba(255, 255, 255, 0.95);
    --primary-color: #0288D1;
    --secondary-color: #F0F9FF;
    --text-color: #0F172A;
    --border-color: #E2E8F0;

    --bg-primary: #F8FAFC;
    --bg-secondary: #F1F5F9;
    --bg-tertiary: rgba(2, 136, 209, 0.05);

    --bg-sidebar-top: #E0F2FE;
    --bg-sidebar-bottom: #BAE6FD;
    --bg-sidebar: var(--bg-sidebar-top);

    --bg-card-tint: rgba(2, 136, 209, 0.05);
    --bg-hover: rgba(2, 136, 209, 0.1);
    --bg-active: rgba(2, 136, 209, 0.15);

    --text-primary: #0F172A;
    --text-primary-rgb: 15, 23, 42;
    --text-secondary: #475569;
    --text-muted: #64748B;

    --border-primary: #E2E8F0;
    --border-secondary: #F1F5F9;
    --border-accent: #0288D1;

    --accent-primary: #0288D1;
    --accent-primary-rgb: 2, 136, 209;
    --accent-hover: #075985;

    --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
    --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    --shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    --shadow-glow: 0 0 15px rgba(2, 136, 209, 0.15);

    --text-sidebar-heading: #0F172A;
    --text-sidebar-sub: #475569;
    --text-sidebar-icon: #0288D1;
}"""

sunset_block = """[data-time-theme="sunset"] {
    --bg-color: var(--bg-primary);
    --card-bg: rgba(255, 255, 255, 0.7);
    --bg-card: rgba(255, 255, 255, 0.7);
    --primary-color: var(--accent-primary);
    --secondary-color: var(--bg-secondary);
    --text-color: var(--text-primary);
    --border-color: var(--border-primary);

    --bg-primary: #E17055;
    --bg-secondary: #D63031;
    --bg-tertiary: rgba(225, 112, 85, 0.1);

    --bg-sidebar-top: #E17055;
    --bg-sidebar-bottom: #D63031;
    --bg-sidebar: var(--bg-sidebar-top);

    --bg-card-tint: rgba(225, 112, 85, 0.15);
    --bg-hover: rgba(225, 112, 85, 0.2);
    --bg-active: rgba(225, 112, 85, 0.3);

    --border-accent: #E17055;

    --accent-primary: #E17055;
    --accent-primary-rgb: 225, 112, 85;
    --accent-hover: #D35400;

    --shadow-sm: 0 4px 6px rgba(255, 94, 98, 0.1);
    --shadow-md: 0 8px 16px rgba(255, 94, 98, 0.15);
    --shadow-lg: 0 12px 24px rgba(255, 94, 98, 0.2);
    --shadow-xl: 0 20px 40px rgba(255, 94, 98, 0.3);
    --shadow-glow: 0 0 20px rgba(225, 112, 85, 0.3);

    --text-primary-rgb: 58, 58, 58;
    --text-sidebar-heading: var(--text-white);
    --text-sidebar-sub: rgba(255, 255, 255, 0.8);
    --text-sidebar-icon: var(--text-white);
}"""

night_block = """[data-time-theme="night"] {
    --bg-color: var(--bg-primary);
    --card-bg: rgba(16, 19, 42, 0.7);
    --bg-card: rgba(16, 19, 42, 0.7);
    --primary-color: var(--accent-primary);
    --secondary-color: var(--bg-secondary);
    --text-color: var(--text-primary);
    --border-color: var(--border-primary);

    --bg-primary: #192A56;
    --bg-secondary: #0a0b10;
    --bg-tertiary: rgba(0, 0, 0, 0.2);

    --bg-sidebar-top: #192A56;
    --bg-sidebar-bottom: #0a0b10;
    --bg-sidebar: var(--bg-sidebar-top);

    --bg-card-tint: rgba(0, 0, 0, 0.3);
    --bg-hover: rgba(255, 255, 255, 0.05);
    --bg-active: rgba(255, 255, 255, 0.1);

    --border-primary: rgba(255, 255, 255, 0.1);
    --border-secondary: rgba(255, 255, 255, 0.05);
    --border-accent: #273C75;

    --text-primary: #E0E0E0;
    --text-secondary: #BDBDBD;
    --text-muted: #9E9E9E;
    --text-heading: #FFFFFF;
    --text-input: #FFFFFF;
    --text-select: #FFFFFF;

    --accent-primary: #273C75;
    --accent-primary-rgb: 39, 60, 117;
    --accent-hover: #192A56;

    --shadow-sm: 0 4px 6px rgba(0, 0, 0, 0.3);
    --shadow-md: 0 8px 16px rgba(0, 0, 0, 0.4);
    --shadow-lg: 0 12px 24px rgba(0, 0, 0, 0.5);
    --shadow-xl: 0 20px 40px rgba(0, 0, 0, 0.6);
    --shadow-glow: 0 0 20px rgba(39, 60, 117, 0.3);

    --text-primary-rgb: 224, 224, 224;
    --text-sidebar-heading: var(--text-white);
    --text-sidebar-sub: rgba(255, 255, 255, 0.8);
    --text-sidebar-icon: var(--text-white);
}"""

# Find the point where the theme blocks end or where CSS rules start
# The original file had these blocks at the top.
# I'll replace everything from the start to the first actual CSS rule.
# The first actual CSS rule likely starts after the night_block.

# Actually, the file is now a mess. I'll search for the first occurrence of .btn-primary or similar.
# Or better, just surgically replace the corrupted area.

# Reconstruct the header
header = "\n".join([root_block, morning_block, noon_block, sunset_block, night_block])

# Find where the actual styles begin (usually "/* Global Button Overrides" or something)
split_pattern = re.compile(r'/\* Global Button Overrides', re.IGNORECASE)
match = split_pattern.search(content)

if match:
    remaining_content = content[match.start():]
else:
    # Try another common split point
    split_pattern = re.compile(r'/\* === GLOBAL STYLES === \*/', re.IGNORECASE)
    match = split_pattern.search(content)
    if match:
        remaining_content = content[match.start():]
    else:
        remaining_content = content # Fallback

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(header + "\n\n" + remaining_content)

print("Surgically repaired site.css header and theme blocks.")
