
path = r"d:\Antigravity\JsonCrudApp\Views\Home\Dashboard.cshtml"
with open(path, "r", encoding="utf-8") as f:
    lines = f.readlines()

# Verify markers
start_idx = 156 # Line 157
end_idx = 946 # Line 947

if lines[start_idx].strip() != "<style>":
    print(f"Error: Line {start_idx+1} is not <style>, it is: {lines[start_idx]}")
    exit(1)

if lines[end_idx].strip() != "</style>":
    print(f"Error: Line {end_idx+1} is not </style>, it is: {lines[end_idx]}")
    exit(1)

# Keep lines before start_idx
new_lines = lines[:start_idx]
# Add link
new_lines.append('<link rel="stylesheet" href="~/css/dashboard.css" />\n')
# Add lines after end_idx
new_lines.extend(lines[end_idx+1:])

with open(path, "w", encoding="utf-8") as f:
    f.writelines(new_lines)

print("Successfully refactored CSS out of Dashboard.cshtml")
