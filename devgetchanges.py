import os
import shutil

# Change directory to the user's home directory
os.chdir(os.path.expanduser("~"))

# Define paths
src_base = os.path.join("Documents", "workspace", "microting", "eform-angular-frontend")
dst_base = os.path.join("Documents", "workspace", "microting", "eform-angular-workflow-plugin")

# Remove and copy main directories
paths = [
    (os.path.join("eform-client", "src", "app", "plugins", "modules", "workflow-pn"),
     os.path.join("eform-client", "src", "app", "plugins", "modules", "workflow-pn")),
    (os.path.join("eFormAPI", "Plugins", "Workflow.Pn"),
     os.path.join("eFormAPI", "Plugins", "Workflow.Pn")),
]

for dst_rel_path, src_rel_path in paths:
    dst_path = os.path.join(dst_base, dst_rel_path)
    src_path = os.path.join(src_base, src_rel_path)

    if os.path.exists(dst_path):
        shutil.rmtree(dst_path)

    shutil.copytree(src_path, dst_path)

# List of test files and directories to remove
test_files_to_remove = [
    os.path.join("eform-client", "e2e", "Tests", "workflow-settings"),
    os.path.join("eform-client", "e2e", "Tests", "workflow-general"),
    os.path.join("eform-client", "wdio-headless-plugin-step2.conf.ts"),
    os.path.join("eform-client", "e2e", "Page objects", "Workflow"),
]

# Remove the test files and directories
for rel_path in test_files_to_remove:
    full_path = os.path.join(dst_base, rel_path)
    if os.path.exists(full_path):
        if os.path.isdir(full_path):
            shutil.rmtree(full_path)
        else:
            os.remove(full_path)

# List of test files and directories to copy
test_files_to_copy = [
    (os.path.join("eform-client", "e2e", "Tests", "workflow-settings"),
     os.path.join("eform-client", "e2e", "Tests", "workflow-settings")),
    (os.path.join("eform-client", "e2e", "Tests", "workflow-general"),
     os.path.join("eform-client", "e2e", "Tests", "workflow-general")),
    (os.path.join("eform-client", "wdio-plugin-step2.conf.ts"),
     os.path.join("eform-client", "wdio-headless-plugin-step2.conf.ts")),
    (os.path.join("eform-client", "e2e", "Page objects", "Workflow"),
     os.path.join("eform-client", "e2e", "Page objects", "Workflow")),
]

# Copy the test files and directories
for src_rel_path, dst_rel_path in test_files_to_copy:
    src_path = os.path.join(src_base, src_rel_path)
    dst_path = os.path.join(dst_base, dst_rel_path)

    if os.path.exists(src_path):
        if os.path.isdir(src_path):
            shutil.copytree(src_path, dst_path)
        else:
            shutil.copy2(src_path, dst_path)
