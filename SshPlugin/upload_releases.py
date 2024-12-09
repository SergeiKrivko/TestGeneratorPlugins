import sys
import os
import shutil

from github import Github, Auth


with open("SshPlugin.Api/version.txt", 'r', encoding='utf-8') as f:
    version = f.read().strip()

print(f"Version = {repr(version)}")


# using an access token
auth = Auth.Token(os.getenv("GITHUB_TOKEN"))

# Public Web GitHub
g = Github(auth=auth)

repo = g.get_repo('SergeiKrivko/TestGeneratorPlugins')

release = repo.create_git_release(f"SshApi-{version}", f"SshApi-{version}", '')

for runtime in ['win-x64', 'linux-x64', 'osx-x64']:
    path = f"SshPlugin.Api/bin/Release/net8.0/{runtime}"
    shutil.make_archive(f"{path}/release", 'zip', f"{path}/publish")
    release.upload_asset(f"{path}/release.zip", name=f"ssh-api_{runtime}.zip")
