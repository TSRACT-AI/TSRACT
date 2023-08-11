import argparse
import string
from transformers import LlamaTokenizer

parser = argparse.ArgumentParser()
parser.add_argument('--model_path', type=str, default='openlm-research/open_llama_7b')
args = parser.parse_args()

# Load the tokenizer
tokenizer = LlamaTokenizer.from_pretrained(args.model_path)

# Retrieve the entire vocabulary (token texts and their IDs)
token_data = tokenizer.get_vocab()

# Print each token ID and text on a separate line
for token_text, token_id in token_data.items():
    # Keep only printable characters
    filtered_text = token_text.replace('\u2581', ' ')
    filtered_text = ''.join(ch for ch in filtered_text if ch in string.printable)
    print(f"{token_id}\t{filtered_text}")

print("TSRACT: Finished")
