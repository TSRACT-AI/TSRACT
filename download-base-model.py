from huggingface_hub import snapshot_download

print("Downloading openllama-7b...")
snapshot_download(repo_id = "openlm-research/open_llama_7b", repo_type = 'model')
