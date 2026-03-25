import re
import os

# Path to the file to be modified
file_path = os.path.join("src", "app", "plugins", "plugins.routing.ts")

# Read the content of the file
with open(file_path, "r") as file:
    content = file.read()

# Define the replacements
replacements = [
    (r"// INSERT ROUTES HERE", "  {\n// INSERT ROUTES HERE"),
    (r"// INSERT ROUTES HERE", "    path: 'workflow-pn',\n// INSERT ROUTES HERE"),
    (r"// INSERT ROUTES HERE", "    loadChildren: () => import('./modules/workflow-pn/workflow-pn.module')\n// INSERT ROUTES HERE"),
    (r"// INSERT ROUTES HERE", "      .then(m => m.WorkflowPnModule)\n// INSERT ROUTES HERE"),
    (r"// INSERT ROUTES HERE", "  },\n// INSERT ROUTES HERE"),
]

# Apply each replacement in sequence
for pattern, replacement in replacements:
    content = re.sub(pattern, replacement, content, count=1)

# Write the modified content back to the file
with open(file_path, "w") as file:
    file.write(content)
