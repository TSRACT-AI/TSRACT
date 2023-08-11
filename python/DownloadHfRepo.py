import argparse
from huggingface_hub import snapshot_download

parser = argparse.ArgumentParser()
parser.add_argument('repo_id', type=str)
parser.add_argument('repo_type', type=str)
args = parser.parse_args()

print(f"TSRACT: Downloading...")

snapshot_download(repo_id=args.repo_id, repo_type=args.repo_type)

print(f"TSRACT: Finished")
