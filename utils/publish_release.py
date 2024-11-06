import sys
import os
import json

from github import Github, Auth


path = sys.argv[1]
if len(sys.argv) >= 3:
    arch = sys.argv[2]
else:
    arch = None


with open(f"{path}/Config.json", 'r', encoding='utf-8') as f:
    config = json.loads(f.read())

version = config['Version']
# print(f"Version = {repr(version)}")

# using an access token
auth = Auth.Token(os.getenv("GITHUB_TOKEN"))

# Public Web GitHub
g = Github(auth=auth)

repo = g.get_repo('SergeiKrivko/TestGeneratorPlugins')

release = repo.get_latest_release()
# print(repr(release.tag_name), version)
if release.tag_name != "v" + version:
    release = repo.create_git_release("v" + version, f"Version {version}", '')

if arch:
    name = f"plugin_{arch}.zip"
else:
    name = f"plugin.zip"
asset = release.upload_asset(os.path.join(path, config['Key'] + '.zip'), name)
print(f"url={asset.browser_download_url}")
